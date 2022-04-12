using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class MainController : MonoBehaviour
{
    [SerializeField] RawImage _imgPreview;
    [SerializeField] TMP_InputField _previewFilePath;
    [SerializeField] Button _btnLoad;
    [SerializeField] string[] _supportedExtensions = new string[] { ".png" };

    void InitialAsserts()
    {
        Assert.IsNotNull(_imgPreview);
        Assert.IsNotNull(_previewFilePath);
        Assert.IsNotNull(_btnLoad);
    }

    public void Start()
    {
        InitialAsserts();
        
        _btnLoad.onClick.AddListener(BtnLoad_Click);
    }

    void BtnLoad_Click()
    {
        var filePath = _previewFilePath.text;
        var extension = Path.GetExtension(filePath);
        if (File.Exists(filePath)
            && _supportedExtensions.Any(ext=>ext == extension))
        {
            Texture2D texture = LoadPNG(filePath);
            _imgPreview.texture = texture;
            Debug.Log($"File {filePath} loaded");
        }
        else
        {
            Debug.Log($"Could not load {filePath}");
        }
    }

    public static Texture2D LoadPNG(string filePath)
    {
        Texture2D tex = new Texture2D(1,1);
        byte[] fileData = File.ReadAllBytes(filePath);
        ImageConversion.LoadImage(tex, fileData);
        return tex;
    }
}
