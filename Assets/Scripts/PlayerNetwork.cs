using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerNetwork : MonoBehaviour {

	[SerializeField] private Behaviour[] componentsToDisable;

	private PhotonView photonView;

	private void Start()
	{
		photonView = GetComponent<PhotonView> ();

		Initialize ();
	}

	private void Initialize()
	{
		if (photonView.isMine) {

		} else {
			foreach(Behaviour m in componentsToDisable) {
				m.enabled = false;
			}
		}
	}

	private void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		
	}
}
