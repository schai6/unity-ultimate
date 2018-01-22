using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotonNetworkManager : MonoBehaviour {

	[SerializeField] private GameObject player;
	[SerializeField] private Transform spawnPoint;
	[SerializeField] private GameObject lobbyCamera;
	// Use this for initialization
	void Start () {
		PhotonNetwork.ConnectUsingSettings("A");
	}

	public virtual void OnJoinedLobby()
	{
		Debug.Log ("we have now joined the lobby");
		PhotonNetwork.JoinOrCreateRoom("NewRoom", null, null);
	}

	public virtual void OnJoinedRoom()
	{
		PhotonNetwork.Instantiate (player.name, spawnPoint.position, spawnPoint.rotation, 0);
		lobbyCamera.SetActive (false);
		if (PhotonNetwork.player.isMasterClient) 
		{
			PhotonNetwork.InstantiateSceneObject ("Disc", spawnPoint.position, spawnPoint.rotation, 0, null);
		}
	}
}
