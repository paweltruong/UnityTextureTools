using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MainController : MonoBehaviour
{
    [Header("Preview")]
    [SerializeField] RawImage _imgPreview;
    [SerializeField] TextMeshProUGUI _txtCurrentCoords;
    [SerializeField] TMP_InputField _previewFilePath;
    [SerializeField] Button _btnLoad;
    bool _captureCoords = false;
    [Header("GetPixel")]
    [SerializeField] TMP_InputField _previewPixelX;
    [SerializeField] TMP_InputField _inpComplexString;
    [SerializeField] Button _btnTryParseComplexString;
    [SerializeField] Image _pixelColor;
    [SerializeField] TextMeshProUGUI _pixelColorInfo;
    [SerializeField] TMP_InputField _previewPixelY;
    [SerializeField] Button _btnSampleColor;
    [SerializeField] string[] _supportedExtensions = new string[] { ".png" };

    void InitialAsserts()
    {
        //Preview
        Assert.IsNotNull(_imgPreview);
        Assert.IsNotNull(_txtCurrentCoords);
        Assert.IsNotNull(_previewFilePath);
        Assert.IsNotNull(_btnLoad);

        //GetPixel
        Assert.IsNotNull(_previewPixelX);
        Assert.IsNotNull(_previewPixelY);
        Assert.IsNotNull(_inpComplexString);
        Assert.IsNotNull(_btnTryParseComplexString);
        Assert.IsNotNull(_pixelColor);
        Assert.IsNotNull(_pixelColorInfo);
        Assert.IsNotNull(_btnSampleColor);
    }

    public void Start()
    {
        InitialAsserts();

        _txtCurrentCoords.text = string.Empty;

        _btnLoad.onClick.AddListener(BtnLoad_Click);

        _btnTryParseComplexString.onClick.AddListener(BtnTryParseComplexString_Click);
        _btnSampleColor.onClick.AddListener(BtnSampleColor_Click);
    }

    private void Update()
    {
        if (_captureCoords)
        {
            UpdateMouseCoords();
        }
    }

    void BtnLoad_Click()
    {
        var filePath = _previewFilePath.text;
        var extension = Path.GetExtension(filePath);
        if (File.Exists(filePath)
            && _supportedExtensions.Any(ext => ext == extension))
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

    void BtnTryParseComplexString_Click()
    {

        if (!string.IsNullOrEmpty(_inpComplexString.text))
        {
            int x, y;
            bool successs = false;

            //First method
            if (TryParse1(_inpComplexString.text, out x, out y))
            {
                successs = true;
                Debug.Log("Adv Parse 1 Success");
            }

            if (successs)
            {
                _previewPixelX.text = x.ToString();
                _previewPixelY.text = y.ToString();
                var pixel = (_imgPreview.texture as Texture2D).GetPixel(x, y);
                UpdateSampleColorUI(pixel);
            }
        }
    }

    bool TryParse1(string input, out int x, out int y)
    {
        x = 0;
        y = 0;

        var tmp = input.Trim();
        if (!tmp.StartsWith("xy=(") && tmp.Length > 4) return false;


        var xString = GetFloatString(tmp.Substring(4));
        if (string.IsNullOrEmpty(xString)) return false;

        var secondPart = tmp.Substring(4 + xString.Length + 1/*excpect separtor ,*/);
        var yString = GetFloatString(secondPart);

        if(string.IsNullOrEmpty(yString)) return false;

        if (!float.TryParse(xString, out float xFloat) || !float.TryParse(yString, out float yFloat)) return false;

        x = Mathf.FloorToInt(xFloat);
        y = Mathf.FloorToInt(yFloat);

        return true;
    }

    /// <summary>
    /// f.e from string "7958.354,4960.325)" returns "7958.354"
    /// </summary>
    /// <param name="text"></param>
    /// <returns></returns>
    string GetFloatString(string text)
    {
        string[] startStrings = new string[] { "(" };
        string[] endStrings = new string[] { ",", ")", " " };

        int foundIndex = -1;
        for (int i = 0; i < text.Length; ++i)
        {
            if (endStrings.Any(s => s == text.Substring(i, 1)))
            {
                foundIndex = i;
                break;
            }
        }

        if (foundIndex >= 0)
        {
            var floatString = text.Substring(0, foundIndex);
            return floatString;
        }
        else
        {
            return string.Empty;
        }
    }

    void BtnSampleColor_Click()
    {
        if (_imgPreview.texture == null
           && _imgPreview.texture is Texture2D)
        {
            Debug.LogError("Invalid texture in preview");
            return;
        }

        if (int.TryParse(_previewPixelX.text, out int x)
            && int.TryParse(_previewPixelY.text, out int y)
            && x >= 0 && x < _imgPreview.texture.width
            && y >= 0 && y < _imgPreview.texture.height)
        {
            var pixel = (_imgPreview.texture as Texture2D).GetPixel(x, y);
            UpdateSampleColorUI(pixel);
        }
        else
        {
            Debug.LogError("Invalid pixel coords");
        }
    }

    public static Texture2D LoadPNG(string filePath)
    {
        Texture2D tex = new Texture2D(1, 1);
        byte[] fileData = File.ReadAllBytes(filePath);
        ImageConversion.LoadImage(tex, fileData);
        return tex;
    }

    public void SampleTexturePositionWithCursor(BaseEventData baseEventData)
    {
        if (_imgPreview.texture == null
            && _imgPreview.texture is Texture2D)
        {
            Debug.LogError("Invalid texture in preview");
            return;
        }

        var clickedPixelPosition = MouseToPixelPreviewPosition();
        var pixelX = Mathf.FloorToInt(clickedPixelPosition.x);
        var pixelY = Mathf.FloorToInt(clickedPixelPosition.y);
        _previewPixelX.text = pixelX.ToString();
        _previewPixelY.text = pixelY.ToString();

        var pixel = (_imgPreview.texture as Texture2D).GetPixel(pixelX, pixelY);
        UpdateSampleColorUI(pixel);

        //Debug.Log($"SampleTexturePositionWithCursor SelObj:{baseEventData.selectedObject} , Corners: {string.Join(",", corners)}, Module:{baseEventData.currentInputModule}");
    }

    Vector2 MouseToPixelPreviewPosition()
    {
        Vector3[] corners = new Vector3[4];
        _imgPreview.rectTransform.GetWorldCorners(corners);

        var minX = corners.Select(c => c.x).Min();
        var maxX = corners.Select(c => c.x).Max();

        var minY = corners.Select(c => c.y).Min();
        var maxY = corners.Select(c => c.y).Max();

        var mousePosition = Input.mousePosition;
        var mouseToImageCoordX = Mathf.Clamp(mousePosition.x - minX, 0, maxX - minX);
        var mouseToImageCoordY = Mathf.Clamp(mousePosition.y - minY, 0, maxY - minY);

        var xScale = _imgPreview.texture.width / _imgPreview.rectTransform.rect.width;
        var yScale = _imgPreview.texture.height / _imgPreview.rectTransform.rect.height;

        var mouseToTextureCoordX = xScale * mouseToImageCoordX;
        var mouseToTextureCoordY = yScale * mouseToImageCoordY;

        var clickedPixelPosition = new Vector2(mouseToTextureCoordX, mouseToTextureCoordY);
        return clickedPixelPosition;
    }

    public void UpdateMouseCoords()
    {
        var clickedPixelPosition = MouseToPixelPreviewPosition();
        var pixelX = Mathf.FloorToInt(clickedPixelPosition.x);
        var pixelY = Mathf.FloorToInt(clickedPixelPosition.y);
        _txtCurrentCoords.text = $"({pixelX},{pixelY})";
    }

    public void StartCaptureCoords(BaseEventData baseEventData)
    {
        _captureCoords = true;
        _txtCurrentCoords.text = string.Empty;
    }
    public void StopCaptureCoords(BaseEventData baseEventData)
    {
        _captureCoords = false;
        _txtCurrentCoords.text = string.Empty;
    }

    void UpdateSampleColorUI(Color color)
    {
        _pixelColor.color = color;
        _pixelColorInfo.text = $"{color.ToString()}";

    }
}
