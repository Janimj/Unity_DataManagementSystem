# DataManagement System for Unity, version 0.1.0

## Description:
A tool to ease the management of data within a Unity project.
Fill in the gui based fields to determine how any serializable class is handled. Where is the data going to be stored? Which format does the stored data use? Does it need to be encrypted? Do you need its hash to match for a reason or another?
Forgot where certain data is stored? Just pop open the gui and have a look!

## Features:
* Serializes any class marked with [System.Serializable] to a chosen format. Currently available formats are json, xml and binary.
* Decide where the save file will be generated and the file extension to be used. (Presets for persistentDataPath and Assets/Resources folder if those are not suitable, just setup custom path to anywhere.)
* Add hash of the data at the end of the file. Currently available hash choices are MD5, SHA256, SHA512
* Add encryption to use for the file and set the encyption key (currently support simple XOR)

## Usage:
Open the window within Unity toolbar -> Utility -> DataManagement
Click the Add item button, fill in the dataclass name, name of the save file, location of the save file and any other options you deem necessary. Click Save button which will add your to managed classes. You can view managed classes by pressing the Show Items button, expanding each class will allow to view its current settings and provides ability to edit them or delete the class from managed classes. Show  class data shows the data structure of the class.

Generic load and save functions allow loading data with simply typing:
myType = DataManager.Load(myType) or myType = DataManager.Load(myType, myEncryptionKey)
DataManager.Save(myType) or DataManager.Save(myType, myEncryptionKey)

This should only be used during development and separate save / load functions should be written for the production version. As it is neither secure nor optimized yet.

## Future plans:
* Cloud based saving/loading implementation via http methods
* Production viable version
* More encryption algorithm options
* More data format to utilize
* Ability to create custom data formats
* Data Groups, save / load everything at once within a group

## Some questions that might rise:
### Why Assets/Resources folder?
Resources folder is included in the build version of a Unity project, storing it in assets folder wont allow its usage with build versions. 

### Why isnt the settings file in the Assets/Resources folder then?
It shouldnt be used with build version, as the current version lacks some key functionality that most likely break it. Its also not secure as it would be in plain text format and anyone with the build version could modify any of the settings data breaking the build of the build was relying on it. If using it for development build testing, just copy it to the resources folder before building or into the builds /data folder, but keep in mind that it might not work. Writing separate loading / saving functions is advised.

### Whats simple XOR?
Not Vernam Cipher xor implementation. Simple int based key to displace all of the characters. Not secure, easy to crack unlike vernam cipher. Its more about obscurity to prevent simplest save data manipulation.

### Why my _fill in the plank_ was not serialized?
Some types cannot be serialized, Dictionary for example is not serialisable. Properties are not serialized either. Serialization also requires either public access level for the variable or [SerializeField] unity tag. 
#### Further reading here: 
* https://docs.unity3d.com/Manual/script-Serialization.html
* https://docs.microsoft.com/en-us/dotnet/standard/serialization/
