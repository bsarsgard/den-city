using UnityEngine;
using System.Collections;
using DenCity;
 
namespace DenCity.Tiles {
    public class Building: Tile {
		public int Population { get; set; }
		
		public Building() {
			this.Population = 1;
		}
		
		public void Grow() {
			this.Population += 1;
			this.TileObject.transform.localScale = new Vector3(this.TileObject.transform.localScale.x, Mathf.Sqrt(this.Population), this.TileObject.transform.localScale.z);
			this.TileObject.transform.position = new Vector3(this.TileObject.transform.position.x, this.TileObject.transform.localScale.y / 2f, this.TileObject.transform.position.z);
		}
    }
}