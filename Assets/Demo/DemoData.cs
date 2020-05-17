using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DemoData
{
    public string SomeString;
    public float SomeFloat;
    public int SomeInt;
}
[Serializable]
public class DemoDataXML
{
    public string SomeString;
    public float SomeFloat;
    public int SomeInt;
}
[Serializable]
public class DemoDataBinary
{
    public string SomeString;
    public float SomeFloat;
    public int SomeInt;
}
[Serializable]
public class DemoDataHashed
{
    public string SomeString;
    public float SomeFloat;
    public int SomeInt;
}
[Serializable]
public class DemoDataEncrypted
{
    public string SomeString;
    public float SomeFloat;
    public int SomeInt;
}
[Serializable]
public class DemoDataEncryptedHashed
{
    public string SomeString;
    public float SomeFloat;
    public int SomeInt;
}

[Serializable]
public class DemoDataUnityVars
{
    public Vector3 SomeVector3;
    public GameObject SomeGameObject;
    public Transform SomeTransform;
}