using System.IO;
using UnityEngine;
using System.Collections;

public class TableDataReadTest : MonoBehaviour {

	// Use this for initialization
	void Start ()
	{
	    byte[] bytes = File.ReadAllBytes(Application.dataPath + "/AIFrame/Resources/roleInfo.kiss");
	   roleInfoTableManager.instance.LoadData(bytes);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnGUI()
    {
        for (int i = 0; i < roleInfoTableManager.instance.Size(); i++)
        {
            roleInfo info = roleInfoTableManager.instance.GetByIndex(i);
            GUILayout.Label(info.ID + info.name + info.resModel);
        }
    }

}
