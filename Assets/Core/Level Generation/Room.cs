﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;


public abstract class RoomObjectBehaviour : MonoBehaviour {
	Room _room;
	protected Roomlike room {
		get { // If root is a room, return root. Otherwise if there is a Room.active, return that. Otherwise phantom.
			if (!_room)
				_room = transform.root.GetComponent<Room>();
			return (Roomlike)(_room ?? Room.active) ?? PhantomRoom.active;
		}
	}

	protected WaveEngine waveEngine { get => room.waveEngine; }

	public virtual void Configure(string[] args) { }

	bool prePauseEnablementState;
	public void Pause() {
		prePauseEnablementState = enabled;
		enabled = false;
	}
	public void Unpause() {
		enabled = prePauseEnablementState;
	}
}

public interface Configurable {
	void Configure(string[] args); // Note that first argument is gameObject-label.component-name
}

[Serializable]
public enum BorderType { Closed = 0, Free = 1, Absorb = 2 }

[Serializable]
public class BorderData {
	public BorderType type = BorderType.Absorb;
	public float thickness = 0.5f;
	public bool door = true;
	public static BorderData operator &(BorderData border, bool andDoor) {
		return new BorderData {
			type = border.type,
			thickness = border.thickness,
			door = border.door & andDoor
		};
	}
}

[Serializable]
public class ObjectData {

	public string templateName;

	GameObject _template;
	public GameObject template { get => _template ?? (_template = (GameObject)Resources.Load(templateName)); }
	public GameObject instance;

	public float x = 0;
	public float y = 0;
	public Vector2 position { get => new Vector2(x, y); }

	public float xScale = 1;
	public float yScale = 1;
	public Vector2 scale { get => new Vector2(xScale, yScale); }

	public float angle = 0;

	public string label;
	public ConfigData[] configs = new ConfigData[0];

}

[Serializable]
public struct ConfigData {
	public string receptor;
	public string[] args;
	public RoomObjectBehaviour ParseReceptor(Transform root) {
		// Format: root.child[...].child.component.defaultEnablementState
		string[] parts = receptor.Split('.');
		for (int j = 0; j < parts.Length - 1; j++)
			root = root.Find(parts[j]);
		return (RoomObjectBehaviour)root.gameObject.GetComponent(parts[parts.Length - 1]);
	}
}

[Serializable]
public class RoomData {
	public string name = "Empty";
	public float width = 12;
	public float height = 12;
	public Vector2 size { get => new Vector2(width, height); }
	public SimulationParams simParams;
	public BorderData[] borders = new BorderData[4] { new BorderData(), new BorderData(), new BorderData(), new BorderData() };
	public ObjectData[] objects = new ObjectData[0];
}

public interface Roomlike {
	WaveEngine waveEngine { get; }
	Vector2 size { get; }
	float timeScale { get; }
	float deltaTime { get; }
}

public class PhantomRoom : Roomlike {
	public WaveEngine waveEngine { get => WaveEngine.active; }
	public Vector2 size { get => waveEngine.transform.localScale; }
	public float timeScale { get => waveEngine ? waveEngine.param.timeScale : 1; }
	public float deltaTime { get => waveEngine ? waveEngine.param.deltaTime : Time.deltaTime; } // waveEngine.param.deltaTime is a property = waveEngine.param.timeScale * Time.deltaTime
	public static PhantomRoom active = new PhantomRoom();
}

public class Room : MonoBehaviour, Roomlike {

	public static Room active;
	public static Action<Room> OnExitRoom = delegate { };
	public static Action<Room> OnEnterRoom = delegate { };

	public bool[] doors = new bool[] { false, false, false, false };

	public string importPath;
	public string exportPath = "~/Test";
	public bool import;
	public bool export;
	public RoomData data;

	public Vector2 size { get => data.size; }
	public float timeScale { get => data.simParams.timeScale; }
	public float deltaTime { get => data.simParams.deltaTime; }

	public GameObject waveEngineTemplate;
	public GameObject borderTemplate;
	WaveEngine _waveEngine;
	public WaveEngine waveEngine { get => _waveEngine; }

	// Named room objects have two purposes:
	// 1) Names are used to find targets of config commands
	// 2) Named objects are tracked and exposed in the named Dictionary
	public Dictionary<string, GameObject> named = new Dictionary<string, GameObject>();

	public Rect rect {
		get => new Rect((Vector2)transform.position - size / 2 - Vector2.one * Border.exteriorThickness, size + Vector2.one * Border.exteriorThickness * 2);
	}

	public void ResetRoom() {
		gameObject.BroadcastMessage("Reset", SendMessageOptions.DontRequireReceiver);
	}

	public void PauseRoom() {
		gameObject.BroadcastMessage("Pause", SendMessageOptions.DontRequireReceiver);
	}

	public void UnpauseRoom() {
		gameObject.BroadcastMessage("Unpause", SendMessageOptions.DontRequireReceiver);
	}

	public void ImportData(string text) {
		for (int i = 0; i < 4; i++)
			data.borders[i].door = data.borders[i].door && doors[i];
	}

	public void ReloadRoom(bool dontDestroy = false) {

		// Destroy all instantiated objects (except WavePlane)
		if (!dontDestroy)
			for (int i = 0; i < transform.childCount; i++)
				Destroy(transform.GetChild(i).gameObject);

		// Clear named dictionary
		named.Clear();

		// Instance wave engine and plane
		_waveEngine = Instantiate(waveEngineTemplate, transform).GetComponent<WaveEngine>();
		waveEngine.transform.localPosition = Vector3.forward;
		waveEngine.param = data.simParams;
		waveEngine.transform.localScale = new Vector3(data.width, data.height, 1);
		waveEngine.Initialize();

		// Instance room borders
		for (int i = 0; i < 4; i++)
			Instantiate(borderTemplate, transform).GetComponent<Border>().Draw(data.borders[i] & doors[i], i);


		// Instance enemies
		foreach (ObjectData obj in data.objects) {
			obj.instance = Instantiate(obj.template, obj.position + (Vector2)transform.position, Quaternion.Euler(0, 0, obj.angle) * transform.rotation, transform);
			obj.instance.transform.localScale = new Vector3(obj.scale.x, obj.scale.y, 1);
			if (obj.label != null && obj.label != "") named.Add(obj.label, obj.instance);
		}

		// Confingure enemies. Must do this after all enemies instance.
		foreach (ObjectData obj in data.objects)
			foreach (ConfigData config in obj.configs)
				config.ParseReceptor(obj.instance.transform).Configure(config.args);

		if (active == this) {
			waveEngine.SetActive();
		} else {
			PauseRoom();
		}
	}

	public Component ParseNamedReference(string str, int clipEnd = 0) {
		return ParseNamedReference(str, clipEnd, out string[] discard);
	}

	public Component ParseNamedReference(string str, int clipEnd, out string[] clippings) {
		string[] parts = str.Split('.');
		Transform obj = named[parts[0]].transform;
		for (int j = 1; j < parts.Length - clipEnd - 1; j++)
			obj = obj.Find(parts[j]);
		clippings = parts.Skip(parts.Length - clipEnd).ToArray();
		return obj.gameObject.GetComponent(parts[parts.Length - clipEnd - 1]);
	}

	public void Import() {
		string text = Resources.Load<TextAsset>("Rooms/" + importPath).text;
		if (text != null)
			data = JsonUtility.FromJson<RoomData>(text);
	}

	public void Export() {
		string text = JsonUtility.ToJson(data);
		string path = exportPath.StartsWith("~") ? exportPath.Replace("~", "Assets/Resources/Rooms") : exportPath;
		System.IO.File.WriteAllText(path, text);
	}

	public void Awake() {
		if (importPath != "") {
			Import();
		} else {
			active = this;
		}
		ReloadRoom(importPath == "");
	}

	private void Update() {

		if (Input.GetKeyDown(KeyCode.R)) {
			if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
				ReloadRoom();
			else
				ResetRoom();
		}

		if (import) {
			import = false;
			Import();
		}
		if (export) {
			export = false;
			Export();
		}


		// Manage activeness

		if (Player.instance == null)
			return;

		bool containsPlayer = rect.Contains(Player.instance.transform.position);

		if (containsPlayer & active == null) {
			active = this;
			UnpauseRoom();
			waveEngine.SetActive();
			OnEnterRoom(this);
		}
		if (!containsPlayer && active == this) {
			PauseRoom();
			active = null;
			OnExitRoom(this);
		}

	}

}
