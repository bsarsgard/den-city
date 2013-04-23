using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DenCity;
using DenCity.Tiles;

public class MainThread : MonoBehaviour {
	
	// objects and prefabs
	public GameObject cursorPrefab;
	public GameObject residentialCubePrefab;
	public GameObject roadPrefab;
	
	private GameObject activeCursor;
	
	private bool isInitialized = false;
	private TerrainGeneratorBehaviorScript terrainGenerator;
	
	// Holds the board heights
	private float[,] heightMap;
	
	// Holds game tiles
	private Dictionary<Vector2, Tile> tileMap;
	
	private ArrayList buildings = new ArrayList();
	
	// Gui
	private Rect menuBox;
	private int selectionGridInt = -1;
		
	string[] selectionStrings = {"Clear", "Road", "Residential"};
	
	// Awake initializes before other GameObjects run Start
	void Awake() {
		// set up the gui
		menuBox = new Rect(10,10,100,200);
	}

	// Use this for initialization
	void Initialize () {
		// Initialize terrain
		terrainGenerator = GetComponent<TerrainGeneratorBehaviorScript>();
		this.heightMap = terrainGenerator.HeightMap;
		this.tileMap = new Dictionary<Vector2, Tile>();
		
		// Resize objects
		roadPrefab.transform.localScale = new Vector3(terrainGenerator.TileWide / 10f, 1, terrainGenerator.TileHigh / 10f);
		cursorPrefab.transform.localScale = new Vector3(terrainGenerator.TileWide / 10f, 1, terrainGenerator.TileHigh / 10f);
		residentialCubePrefab.transform.localScale = new Vector3(terrainGenerator.TileWide, terrainGenerator.TileHigh, terrainGenerator.TileHigh);
		
		this.isInitialized = true;
	}
	
	// Update is called once per frame
	void Update () {
		if (!isInitialized) {
			this.Initialize();
		}
		
		// Get on-screen mouse transltion
		float ix = Input.mousePosition.x;
		float iy = Input.mousePosition.y;
		Vector3 transMouse = GUI.matrix.inverse.MultiplyPoint3x4(new Vector3(ix, Screen.height - iy, 1));
		
		// Execute UI if the mouse is not over the menu box
		if (!menuBox.Contains(new Vector2(transMouse.x, transMouse.y))) {
		    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		    RaycastHit hit = new RaycastHit();
		    if (Physics.Raycast(ray, out hit))
		    {
				// Check mouse
				if (Input.GetMouseButtonUp(0)) // LMB Clicked
				{
					// Add object at cursor
					Vector3 cubePoint = new Vector3(hit.point.x, hit.point.y, hit.point.z);
					cubePoint.y += 0.1f;
					cubePoint.x -= (cubePoint.x % terrainGenerator.TileWide) - terrainGenerator.TileWide / 2f;
					cubePoint.z -= (cubePoint.z % terrainGenerator.TileHigh) - terrainGenerator.TileHigh / 2f;
					
					Vector2 tilePoint = new Vector2(Mathf.FloorToInt(cubePoint.x / terrainGenerator.TileWide), Mathf.FloorToInt(cubePoint.z / terrainGenerator.TileHigh));
					Tile tile = null;
					
					switch (selectionStrings[selectionGridInt]) {
					case "Road":
						tile = new Road();
						break;
					case "Residential":
						tile = new Building();
						break;
					case "Clear":
						if (tileMap.ContainsKey(tilePoint)) {
							Destroy(tileMap[tilePoint].TileObject);
							tileMap.Remove(tilePoint);
						}
						break;
					default:
						break;
					}
					if (tile != null && !tileMap.ContainsKey(tilePoint)) {
						tile.TileObject = Instantiate(activeCursor, cubePoint, Quaternion.identity);
						tileMap.Add (tilePoint, tile);
					}
				}
				else
				{
					// Draw cursor
					Vector3 cursorPoint = new Vector3(hit.point.x, hit.point.y, hit.point.z);
					cursorPoint.x -= (cursorPoint.x % terrainGenerator.TileWide) - terrainGenerator.TileWide / 2f;
					cursorPoint.z -= (cursorPoint.z % terrainGenerator.TileHigh) - terrainGenerator.TileHigh / 2f;
					cursorPrefab.transform.position = cursorPoint;
				}
			}
		}
	}
	
	// Draw the UI
	void OnGUI () {
		// Make a background box
		GUI.Box(menuBox, "Build Menu");
		
		/*
		if (GUI.Toggle(new Rect(20,40,80,20), activeCursor == roadPrefab, "Road")) {
			Debug.Log ("Button 1");
			activeCursor = roadPrefab;
		}

		if (GUI.Button(new Rect(20,70,80,20), "Level 2")) {
			Debug.Log ("2");
		}
		*/

		selectionGridInt = GUI.SelectionGrid (new Rect (20, 40, 80, 60), selectionGridInt, selectionStrings, 1);
		
		if (selectionGridInt > 0) {
			switch (selectionStrings[selectionGridInt]) {
			case "Road":
				activeCursor = roadPrefab;
				break;
			case "Residential":
				activeCursor = residentialCubePrefab;
				break;
			case "Clear":
			default:
				activeCursor = null;
				break;
			}
		}
	}
	
	/*
    bool GoodButton(Rect bounds, string caption) {
        GUIStyle btnStyle = GUI.skin.FindStyle("button");
        int controlID = GUIUtility.GetControlID(bounds.GetHashCode(), FocusType.Passive);
       
        bool isMouseOver = bounds.Contains(Event.current.mousePosition);
        bool isDown = GUIUtility.hotControl == controlID;
 
        if (GUIUtility.hotControl != 0 && !isDown) {
            // ignore mouse while some other control has it
            // (this is the key bit that GUI.Button appears to be missing)
            isMouseOver = false;
        }
       
        if (Event.current.type == EventType.Repaint) {
            btnStyle.Draw(bounds, new GUIContent(caption), isMouseOver, isDown, false, false);
        }
        switch (Event.current.GetTypeForControl(controlID)) {
            case EventType.mouseDown:
                if (isMouseOver) {  // (note: isMouseOver will be false when another control is hot)
                    GUIUtility.hotControl = controlID;
                }
                break;
               
            case EventType.mouseUp:
                if (GUIUtility.hotControl == controlID) GUIUtility.hotControl = 0;
                if (isMouseOver && bounds.Contains(Event.current.mousePosition)) return true;
                break;
        }
 
        return false;
    }
	*/
}
