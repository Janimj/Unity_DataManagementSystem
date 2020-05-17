using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using JaniMJ.DataManager.Local;
using JaniMJ.DataManager.Settings;


public class DataEditor : EditorWindow
{
    // View / Flow control
    private bool _showItems;
    private bool _addItem;
    private bool _editItem;
    private bool _deletingItem;
    private bool[] _expandedChildren;
    private bool[] _showClassData;

    // Currently being edited or marked for deletion
    private int _editIndex;

    // Adding / Editing data
    private DataHash _hash = DataHash.None;
    private DataEncryption _encryption = DataEncryption.None;
    private DataModel _dataModel = DataModel.Json;
    private string _className;
    private LocalLocation _dataLocation;
    private string _path;
    private string _extension = ".json";
    private string _fileName;
    private bool _jsonPrettyPrint;

    private List<DataType> _typedata;
    private static EditorWindow _window;

    // Titles, Tooltips
    private GUIContent _addButtonContent = new GUIContent("Add Item",
        "Add new data classes to include into the data management.");
    private GUIContent _showButtonContent = new GUIContent("Show Items",
        "Show/Hide data types managed by the datamanagement system.");
    private GUIContent _classContent = new GUIContent("Class", "Name of the class to use.");
    private GUIContent _classPathContent = new GUIContent("...", "Browse to the path file");
    private GUIContent _hashContent = new GUIContent("Hash Type", "Which hash algorithm to use for the hash.");
    private GUIContent _encryptionContent = new GUIContent("Encryption type", "Encryption algorithm to use.");
    private GUIContent _encryptionTextContent = new GUIContent("Encryption key", "Encryption key to use.");
    private GUIContent _filenameContent = new GUIContent("File name", "Name of the save file.");
    private GUIContent _dataModelContent = new GUIContent("Data Model", "Data model to use for the serialization.");
    private GUIContent _deleteButtonContent = new GUIContent("Delete", "Delete the data class from managed data classes.");
    private GUIContent _editButtonContent = new GUIContent("Edit", "Edit the data class settings.");
    private GUIContent _fileExtContent =
        new GUIContent("File Extension", "File extension to be used on the save file.");

    private float scrollValue;

    [MenuItem("Utility/DataManagement")]
    public static void ShowWindow()
    {
        _window = GetWindow(typeof(DataEditor));
        _window.titleContent.text = "Data Management";
        _window.minSize = new Vector2(340,100);
    }

    private Vector2 scrollPos = Vector2.zero;

    private void OnEnable()
    {
        if (_expandedChildren == null)
        {
            _expandedChildren = new bool[0];
        }

        if (_showClassData == null)
        {
            _showClassData = new bool[0];
        }
    }
    private void OnGUI()
    {
        scrollPos = GUILayout.BeginScrollView(scrollPos, false, true, GUILayout.Width(_window.position.width),
            GUILayout.Height(_window.position.height));
        if (OnDeleteItem()) return;

        _typedata = DataManager.DataTypes.DataTypesList;

        UpdateChildSelectionArrays();

        if (_addItem || _editItem)
        {
            OnModifyData();
        }
        else
        {
            DrawBasicView();
        }
        GUILayout.EndScrollView();
    }

    /// <summary>
    /// Draws the "basic" gui view to the screen
    /// </summary>
    private void DrawBasicView()
    {
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(_addButtonContent, GUILayout.Height(50),
            GUILayout.Width(_window.position.xMax / 2 - _window.position.xMax * 0.07f)))
        {
            _addItem = true;
        }

        GUILayout.FlexibleSpace();
        _showButtonContent.text = _showItems ? "Hide Items" : "Show Items";
        if (GUILayout.Button(new GUIContent(_showButtonContent), GUILayout.Height(50),
            GUILayout.Width(_window.position.xMax / 2 - _window.position.xMax * 0.07f)))
        {
            _showItems = !_showItems;
        }

        GUILayout.EndHorizontal();
        if (_showItems)
        {
            OnShowItems();
        }
    }

    /// <summary>
    /// Updates the boolean arrays that track which child and which child data is expanded
    /// </summary>
    private void UpdateChildSelectionArrays()
    {
        if (_expandedChildren.Length == _typedata.Count) return;

        Array.Resize(ref _expandedChildren, _typedata.Count);
        Array.Resize(ref _showClassData, _typedata.Count);
        for (int i = 0; i < _expandedChildren.Length; i++)
        {
            _expandedChildren[i] = false;
            _showClassData[i] = false;
        }
    }

    /// <summary>
    /// Creating new data type or editing existing data type
    /// </summary>
    private void OnModifyData()
    {
        GUILayout.Label(_addItem ? "Add new data" : "Editing: " + DataManager.DataTypes.DataTypesList[_editIndex].Class, EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        _className = EditorGUILayout.TextField(_classContent, _className);
        if (GUILayout.Button(_classPathContent, GUILayout.Width(20), GUILayout.Height(20)))
        {
            string name = "";
            try
            {
                name = new FileInfo(EditorUtility.OpenFilePanel("Path to class", Application.dataPath, "")).Name;
            }
            catch (Exception)
            {
                OnError("Path to class was empty...");
            }

            _className = name != "" ? name.Substring(0, name.IndexOf(".")) : "";
        }

        GUILayout.EndHorizontal();

        _fileName = EditorGUILayout.TextField(_filenameContent, _fileName);
        EditorGUI.BeginChangeCheck();
        
        GUILayout.BeginHorizontal();

        
        _dataModel = (DataModel)EditorGUILayout.Popup(_dataModelContent, (int)_dataModel,
            Enum.GetNames(typeof(DataModel)));
        
        if (_dataModel == DataModel.Json)
        {
            _jsonPrettyPrint = GUILayout.Toggle(_jsonPrettyPrint, "Pretty print");
        }
        GUILayout.EndHorizontal();
        
        if (EditorGUI.EndChangeCheck())
        {
            UpdateExtension();
        }
        
        _extension = EditorGUILayout.TextField(_fileExtContent, _extension);
        _dataLocation = (LocalLocation)EditorGUILayout.EnumPopup("Data Location", _dataLocation);
        if (_dataLocation == LocalLocation.CustomPath)
        {
            EditorGUILayout.Separator();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Path:", GUILayout.Width(148));
            _path = GUILayout.TextArea(_path);
            if (GUILayout.Button("...", GUILayout.Width(20), GUILayout.Height(20)))
            {
                string path = "";
                try
                {
                    path = new FileInfo(EditorUtility.OpenFolderPanel("Save folder", Application.dataPath, "")).ToString();
                }
                catch (Exception)
                {
                    OnError("Path is invalid...");
                }

                _path = path;
            }

            GUILayout.EndHorizontal();
            EditorGUILayout.Separator();
        }

        _hash = (DataHash)EditorGUILayout.Popup(_hashContent, (int)_hash, Enum.GetNames(typeof(DataHash)));
        _encryption = (DataEncryption)EditorGUILayout.Popup(_encryptionContent, (int)_encryption,
            Enum.GetNames(typeof(DataEncryption)));

        EditorGUILayout.Separator();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Save", GUILayout.Height(50)))
        {
            OnSaveButton();
        }

        if (GUILayout.Button("Discard", GUILayout.Height(50)))
        {
            ResetValues();
        }
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// When the save button is pressed runs checks if the data is valid and throws errors if not, save to datatypes
    /// </summary>
    private void OnSaveButton()
    {
        var test = DataManager.GetType(_className);
        Debug.Log(_className);
        Debug.Log(test);
        // If no class is found with the given name, "throw" error
        if (test == null)
        {
            OnError("Class is null...");
            return;
        }
        // If a class with the given name is not serializable, "throw" error
        if (!test.IsSerializable)
        {
            OnError("Class is not serializable");
            return;
        }

        if (!_editItem)
        {
            if (DataManager.DataTypes.DataTypesList.Exists(item => item.Class == _className))
            {
                OnError("Class settings already exist...");
                return;
            }
        }

        // If the file name is empty, "throw" error
        if (_fileName == "")
        {
            OnError("File name is empty");
            return;
        }

        if (_dataLocation == LocalLocation.CustomPath && !Directory.Exists(_path))
        {
            OnError("Path is invalid...");
            return;
        }
        // If the file extension is empty, notify as "warning" and still allow
        if (_extension == "")
        {
            Debug.Log("Warning: File extension was empty...");
        }

        //TODO Addition of Aes
        /*if (_encryption == DataEncryption.Aes256)
        {
            _errorText = "Encryption type is not implemented yet";
            Debug.Log(_errorText);
            return;
        }*/
        // Create the data type and add it to datatypes

        if (_dataLocation == LocalLocation.PersistentDataPath)
        {
            _path = Application.persistentDataPath;
        }

        if (_dataLocation == LocalLocation.ResourcesPath)
        {
            _path = Path.Combine(Application.dataPath + "/", "Resources");
        }
        if (!_editItem)
        {
            DataManager.DataTypes.DataTypesList.Add(new DataType
            {
                Class = _className,
                FileName = _fileName,
                Extension = _extension,
                Path = _path,
                LocalLoc = _dataLocation,
                Hash = _hash,
                Encryption = _encryption,
                Model = _dataModel,
                JsonPrettyPrint = _jsonPrettyPrint
            }
            );
        }
        // Create new data type and replace the type that was being edited
        else
        {
            DataManager.DataTypes.DataTypesList[_editIndex] = new DataType
            {
                Class = _className,
                FileName = _fileName,
                Extension = _extension,
                Path = _path,
                LocalLoc = _dataLocation,
                Hash = _hash,
                Encryption = _encryption,
                Model = _dataModel,
                JsonPrettyPrint = _jsonPrettyPrint
            };
        }

        // Save the datatypes and reset to start
        DataManager.SaveDataTypes();
        _addItem = false;
        ShowNotification(new GUIContent("Data settings stored."), 2);
        ResetValues();
    }

    /// <summary>
    /// Reset variables to default state
    /// </summary>
    private void ResetValues()
    {
        _addItem = false;
        _editItem = false;
        _className = "";
        _fileName = "";
        _dataLocation = LocalLocation.PersistentDataPath;
        _hash = DataHash.None;
        _encryption = DataEncryption.None;
        _dataModel = DataModel.Json;
        _extension = ".json";
        _path = "";
        _editIndex = -1;
        _jsonPrettyPrint = false;
    }

    /// <summary>
    /// Draw managed datatypes to screen when showing items
    /// </summary>
    private void OnShowItems()
    {
        for (int i = 0; i < _typedata.Count; i++)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.BeginHorizontal();
            GUILayout.Label(_typedata[i].Class, EditorStyles.boldLabel);

            if (GUILayout.Button(new GUIContent(_expandedChildren[i]
                        ? "-"
                        : "+",
                    "Show / Hide data type settings."),
                GUILayout.Width(30),
                GUILayout.Height(30)))
            {
                _expandedChildren[i] = !_expandedChildren[i];
            }

            GUILayout.EndHorizontal();

            if (!_expandedChildren[i]) continue;

            OnShowExpandedChildrenView(i);

            if (GUILayout.Button(new GUIContent(_showClassData[i]
                    ? "Hide class data"
                    : "Show class data",
                "Show / Hide class data structure.")))
            {
                _showClassData[i] = !_showClassData[i];
            }

            if (!_showClassData[i]) continue;

            OnShowChildDataStructure(i);
        }
    }

    /// <summary>
    /// Called if the childs toggle to show class data is open
    /// </summary>
    /// <param name="i"> Child index </param>
    private void OnShowChildDataStructure(int i)
    {
        var t = Type.GetType(_typedata[i].Class);
        var fields = t?.GetFields();

        for (int j = 0; j < fields.Length; j++)
        {
            GUILayout.Label(fields[j].FieldType + " | " + fields[j].Name);
        }
    }

    /// <summary>
    /// Called if the child is set to expand
    /// </summary>
    /// <param name="i"> Child index </param>
    private void OnShowExpandedChildrenView(int i)
    {
        GUILayout.Label("File Name: " + _typedata[i].FileName);
        GUILayout.Label("Extension: " + _typedata[i].Extension);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Path:");
        GUILayout.Label(_typedata[i].Path, "TextArea");
        GUILayout.EndHorizontal();
        GUILayout.Label("Data model: " + _typedata[i].Model);
        if (_typedata[i].Model == DataModel.Json)
        {
            GUILayout.Label("Pretty print: " + _jsonPrettyPrint);
        }
        GUILayout.Label("Hash: " + _typedata[i].Hash);
        GUILayout.Label("Encryption: " + _typedata[i].Encryption);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button(_editButtonContent, GUILayout.Height(50), GUILayout.Width(80)))
        {
            _editIndex = i;
            OnEditButton(_typedata[i]);
        }

        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Copy Path", GUILayout.Height(50), GUILayout.Width(80)))
        {
            GUIUtility.systemCopyBuffer = _typedata[i].Path;
            ShowNotification(new GUIContent("Copied to clipboard"), 2);
        }

        GUILayout.FlexibleSpace();
        if (GUILayout.Button(_deleteButtonContent, GUILayout.Height(50), GUILayout.Width(80)))
        {
            _deletingItem = true;
            _editIndex = i;
        }

        GUILayout.EndHorizontal();
    }

    /// <summary>
    /// If Delete was pressed on any of the data items
    /// </summary>
    /// <returns> If waiting for deletion or cancellation </returns>
    private bool OnDeleteItem()
    {
        if (!_deletingItem) return false;
        GUILayout.Label("Delete " + _typedata[_editIndex].Class + "?");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Confirm", GUILayout.Height(50)))
        {
            DataManager.DataTypes.DataTypesList.RemoveAt(_editIndex);
            _editIndex = -1;
            _deletingItem = false;
            DataManager.SaveDataTypes();
            ShowNotification(new GUIContent("Data deleted..."), 2);
        }

        if (GUILayout.Button("Cancel", GUILayout.Height(50)))
        {
            _deletingItem = false;
        }
        GUILayout.EndHorizontal();
        GUILayout.EndScrollView();
        return true;
    }

    /// <summary>
    /// Update the file extension field value according to data model selection
    /// </summary>
    private void UpdateExtension()
    {
        switch (_dataModel)
        {
            case DataModel.Json:
                _extension = ".json";
                break;
            case DataModel.Binary:
                _extension = ".txt";
                break;
            case DataModel.XML:
                _extension = ".xml";
                break;
        }
    }

    /// <summary>
    /// Popup error notification and write debug log on error
    /// </summary>
    /// <param name="message"> Error message to popup </param>
    private void OnError(string message)
    {
        ShowNotification(new GUIContent(message), 2);
        Debug.Log(message);
    }

    /// <summary>
    /// Edit data of a data type
    /// </summary>
    /// <param name="data"> Data type to edit </param>
    private void OnEditButton(DataType data)
    {
        _className = data.Class;
        _fileName = data.FileName;
        _dataLocation = data.LocalLoc;
        _path = data.Path;
        _hash = data.Hash;
        _encryption = data.Encryption;
        _editItem = true;
    }
}
