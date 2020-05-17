using System;
using System.Collections;
using System.Collections.Generic;
using JaniMJ.DataManager.Local;
using JaniMJ.DataManager.Settings;
using UnityEngine;
using Random = UnityEngine.Random;

public class TestButton : MonoBehaviour
{
    private void OnHashMissing(DataType type)
    {
        Debug.Log("Hash of file type: " + type.Class + " was missing.");
    }

    private void OnHashMatch(DataType type)
    {
        Debug.Log("Hash of file type: " + type.Class + " matched the stored hash");
    }

    private void OnHashMismatch(DataType type)
    {
        Debug.Log("Hash of file type: " + type.Class + " did not match the stored hash");
    }
    private void OnEnable()
    {
        DataManager.OnHashMismatch = OnHashMismatch;
        DataManager.OnHashMatch = OnHashMatch;
        DataManager.OnHashMissing = OnHashMissing;
    }

    public void OnClick()
    {
        DemoData demoData = new DemoData { SomeFloat = Random.Range(0.0f,1.0f), SomeInt = Random.Range(0,1), SomeString = "Hello"};
        DemoDataXML demoDataXml = new DemoDataXML { SomeFloat = Random.Range(0.0f, 1.0f), SomeInt = Random.Range(0, 1), SomeString = "Darkness" };
        DemoDataBinary demoDataBinary = new DemoDataBinary { SomeFloat = Random.Range(0.0f, 1.0f), SomeInt = Random.Range(0, 1), SomeString = "My" };
        DemoDataHashed demoDataHashed = new DemoDataHashed { SomeFloat = Random.Range(0.0f, 1.0f), SomeInt = Random.Range(0, 1), SomeString = "Old" };
        DemoDataEncrypted demoDataEncrypted = new DemoDataEncrypted { SomeFloat = Random.Range(0.0f, 1.0f), SomeInt = Random.Range(0, 1), SomeString = "Friend" };
        DemoDataEncryptedHashed demoDataEncryptedHashed = new DemoDataEncryptedHashed { SomeFloat = Random.Range(0.0f, 1.0f), SomeInt = Random.Range(0, 1), SomeString = "...." };
        DemoDataUnityVars demoDataUnityVars = new DemoDataUnityVars
        {
            SomeGameObject = new GameObject("GenericName"), SomeVector3 = Vector3.positiveInfinity,
            SomeTransform = GameObject.Find("Main Camera").transform
        };

        DataManager.SaveData(demoData);
        DataManager.SaveData(demoDataXml);
        DataManager.SaveData(demoDataBinary);
        DataManager.SaveData(demoDataHashed);
        DataManager.SaveData(demoDataEncrypted, "123");
        DataManager.SaveData(demoDataEncryptedHashed, "yykaakoo");
        DataManager.SaveData(demoDataUnityVars);

        var loadedDemoData = DataManager.LoadData<DemoData>();
        var loadedDemoDataXml = DataManager.LoadData<DemoDataXML>();
        var loadedDemoDataBinary = DataManager.LoadData<DemoDataBinary>();
        var loadedDemoDataHashed = DataManager.LoadData<DemoDataHashed>();
        var loadedDemoDataEncrypted = DataManager.LoadData<DemoDataEncrypted>("123");
        var loadedDemoDataEncryptedHashed = DataManager.LoadData<DemoDataEncryptedHashed>("yykaakoo");
        var loadedDemoDataUnityVars = DataManager.LoadData<DemoDataUnityVars>();

        Debug.Log("DemoData >> Some float: " +
                  loadedDemoData.SomeFloat +
                  " Some int: " +
                  loadedDemoData.SomeInt +
                  " Some string: " +
                  loadedDemoData.SomeString);
        Debug.Log("DemoDataXML >> Some float: " +
                  loadedDemoDataXml.SomeFloat +
                  " Some int: " +
                  loadedDemoDataXml.SomeInt +
                  " Some string: " +
                  loadedDemoDataXml.SomeString);
        Debug.Log("DemoDataBinary >> Some float: " +
                  loadedDemoDataBinary.SomeFloat +
                  " Some int: " +
                  loadedDemoDataBinary.SomeInt +
                  " Some string: " +
                  loadedDemoDataBinary.SomeString);
        Debug.Log("DemoDataHashed >> Some float: " +
                  loadedDemoDataHashed.SomeFloat +
                  " Some int: " +
                  loadedDemoDataHashed.SomeInt +
                  " Some string: " +
                  loadedDemoDataHashed.SomeString);
        Debug.Log("DemoDataEncrypted >> Some float: " +
                  loadedDemoDataEncrypted.SomeFloat +
                  " Some int: " +
                  loadedDemoDataEncrypted.SomeInt +
                  " Some string: " +
                  loadedDemoDataEncrypted.SomeString);
        Debug.Log("DemoDataEncryptedHashed >> Some float: " +
                  loadedDemoDataEncryptedHashed.SomeFloat +
                  " Some int: " +
                  loadedDemoDataEncryptedHashed.SomeInt +
                  " Some string: " +
                  loadedDemoDataEncryptedHashed.SomeString);
        Debug.Log("DemoDataUnityVars >> Some GameObject: " +
                  loadedDemoDataUnityVars.SomeGameObject +
                  " Some Transform: " +
                  loadedDemoDataUnityVars.SomeTransform +
                  " Some Vector3: " +
                  loadedDemoDataUnityVars.SomeVector3);
    }
}
