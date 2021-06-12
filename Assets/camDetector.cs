using System.Collections;
using System.IO;
using System.Collections.Generic;
using OpenCvSharp;
using OpenCvSharp.Face;
using UnityEngine;
using TMPro;
using System;
using System.Threading;

public class camDetector : MonoBehaviour
{
    // Start is called before the first frame update
    WebCamTexture _webCamTexture;
    CascadeClassifier cascade; // Pretrained classifier from openCV
    OpenCvSharp.Rect MyFace; // Opencv face
    public UnityEngine.UI.RawImage displayVid; // RawImage for webcam texture
    public TextMeshProUGUI camCount; // Text for displaying number of cameras detected
    public TextMeshProUGUI deviceNames; // Text for displaying camera names
    public TextMeshProUGUI faceName; // Text for displaying face detected
    public TextMeshProUGUI trainerInfo; // Text for trainer information
    public UnityEngine.UI.AspectRatioFitter fit; // Aspect ratio fitter for RawImage and webcam
    public int camIndex = 0;
    public bool showModel = false;
    WebCamDevice[] devices;
    void Start()
    {
        
        devices = WebCamTexture.devices;
        // Text meshes for displaying information
        camCount = GameObject.Find("camCount").GetComponent<TextMeshProUGUI>();
        deviceNames = GameObject.Find("deviceNames").GetComponent<TextMeshProUGUI>();
        faceName = GameObject.Find("faceName").GetComponent<TextMeshProUGUI>();
        trainerInfo = GameObject.Find("trainerInfo").GetComponent<TextMeshProUGUI>();

        // Only use this for demo, otherwise comment this section
        GameObject cam = GameObject.Find("camCount");
        GameObject dev = GameObject.Find("deviceNames");
        GameObject face = GameObject.Find("faceName");
        cam.SetActive(false);
        dev.SetActive(false);
        face.SetActive(false);


        camCount.text = "Number of devices: " + devices.Length.ToString(); // Retrieve number of cam devices on your device
        string names = "";
        string path = "";
        
        // Depending on situation, use different xml for face detection in cascade classifiers
        string casFileName = "haarcascade_frontalface_default.xml";
        //string casFileName = "lbpcascade_frontalface.xml";

        // Used for displaying device names
        for(int i = 0; i < devices.Length; i++){
            names += " " + devices[i].name;
            if (i < devices.Length - 1){
                names += ",";
            }
        }

        // Handles different devices
        switch (SystemInfo.deviceType){
            case DeviceType.Desktop: // Only one webcam 
                _webCamTexture = new WebCamTexture(devices[camIndex].name);
                path = Path.Combine(Application.streamingAssetsPath,casFileName);
                break;
            case DeviceType.Handheld: // Multiple cams, you can change depending on front and rear
                camIndex++; 
                _webCamTexture = new WebCamTexture(devices[camIndex].name, Screen.width/8, Screen.height/8);// 1 was my front facing camera
                /*for (int i = 0; i < devices.Length; i++){ // Use this for having alot of cameras
                    if(devices[i].isFrontFacing){
                        _webCamTexture = new WebCamTexture(devices[1].name);
                        break;
                    }
                }*/
                path = Path.Combine(Application.streamingAssetsPath,casFileName);
                WWW reader = new WWW(path);
                while(!reader.isDone){}
                path = Application.persistentDataPath + casFileName;
                File.WriteAllBytes(path, reader.bytes);
                break;
            case DeviceType.Console:
                break;
            case DeviceType.Unknown:
                break;
            default:
                _webCamTexture = new WebCamTexture(devices[camIndex].name);
                path = Path.Combine(Application.streamingAssetsPath,casFileName);
                break;
        }

        deviceNames.text = "Device names:" + names;
        displayVid.texture = _webCamTexture;
        _webCamTexture.Play();
        cascade = new CascadeClassifier(path);
    }
    public int orientation;
    // Update is called once per frame
    void Update()
    {
        UnityEngine.UI.RawImage tempVid;
        Mat frame;
        // Setting aspect ratio
        float ratio = (float)_webCamTexture.width/(float)_webCamTexture.height;
        fit.aspectRatio = ratio;

        // Setting localscale
        float scaleY = _webCamTexture.videoVerticallyMirrored ? -1f : 1f;
        displayVid.rectTransform.localScale = new Vector3(1f, scaleY, 1f);

        // Setting orientation
        orientation = -_webCamTexture.videoRotationAngle;
        displayVid.rectTransform.localEulerAngles = new Vector3(0, 0, orientation);
        
        // Below code was to rotate the texture, however, it did not work as anticipated (Android phones have portrain and landscape angles mixed up)
        /*Texture2D rotatedTexture = new Texture2D(_webCamTexture.width, _webCamTexture.height);
        rotatedTexture.SetPixels32(_webCamTexture.GetPixels32());
        rotatedTexture = rotateTexture(rotatedTexture, true);
        frame = OpenCvSharp.Unity.TextureToMat(rotatedTexture); // Store current frame*/

        frame = OpenCvSharp.Unity.TextureToMat(_webCamTexture);
        findNewFace(frame); // Finds face in frame, face detection feature
        inferFace(frame); // Uses face detection feature to infer a face, face recognition feature
        //display(frame); // Uncomment to display bounding box for face detection
    }
    // Use this method to rotate the texture
    // Android rotates its orientation by -90 degrees and I was unable to get a correct bounding box after rotation
    // Please elaborate from this if you can
    /*Texture2D rotateTexture(Texture2D originalTexture, bool clockwise){
        Color32[] original = originalTexture.GetPixels32();
        Color32[] rotated = new Color32[original.Length];
        int w = originalTexture.width;
        int h = originalTexture.height;

        int iRotated, iOriginal;

        for (int j = 0; j < h; ++j)
        {
            for (int i = 0; i < w; ++i)
            {
                    iRotated = (i + 1) * h - j - 1;
                    iOriginal = clockwise ? original.Length - 1 - (j * w + i) : j * w + i;
                    rotated[iRotated] = original[iOriginal];
            }
        }

        Texture2D rotatedTexture = new Texture2D(h, w);
        rotatedTexture.SetPixels32(rotated);
        rotatedTexture.Apply();
        return rotatedTexture;
    }*/
    // If device is a handheld device, we can use this onclick method to change the camera
    public void changeCamera(){
        Debug.Log("Clicked change camera");
        if(SystemInfo.deviceType == DeviceType.Handheld){
            camIndex = (camIndex + 1) % WebCamTexture.devices.Length;
            _webCamTexture = new WebCamTexture(devices[camIndex].name, Screen.width/8, Screen.height/8);//  was my front facing camera
            displayVid.texture = _webCamTexture;
            _webCamTexture.Play();
        }
    }
    // Detects new face each frame only one face supported, you can use a loop to support multiple faces
    void findNewFace(Mat frame){
        DateTime start = DateTime.Now;
        var faces = cascade.DetectMultiScale(frame, 1.1, 6, HaarDetectionType.ScaleImage);
        DateTime end = DateTime.Now;
        TimeSpan ts = end - start;
        //Debug.Log("Total ms elapsed: " + ts.TotalMilliseconds.ToString());
        if (faces.Length >= 1){
            // Debug.Log(faces[0].Location);
            // float faceY = faces[0].Y;
            // float faceX = faces[0].X;
            MyFace = faces[0]; // Supports only one face
        }
        else{
            showModel = false;
            trainerInfo.text = "No face detected, cannot present trainer information";
        }
    }
    // Method for displaying bounding box of face detection
    void display(Mat frame){
        if (MyFace != null){
            frame.Rectangle(MyFace, new Scalar(250, 0, 0), 2);
        }
        else{
            displayVid.texture = _webCamTexture;
            return;
        }
        Texture newTexture = OpenCvSharp.Unity.MatToTexture(frame);
        //newTexture = (Texture) rotateTexture((Texture2D) newTexture, false);
        displayVid.texture = newTexture;
    }
    // Script used to take a snapshot of your webcam, you can use this method if you want to train from current snapshots
    /*public void snapshot(){
        if (MyFace != null){
            Debug.Log("Snapshot taken");
            string [] imagePaths = {UnityEngine.Application.dataPath, "Images", "test.png"};
            string imagePath = Path.Combine(imagePaths);
            Debug.Log("Where to save: " + imagePath);
            Texture2D photo = new Texture2D(_webCamTexture.width, _webCamTexture.height);
            photo.SetPixels(_webCamTexture.GetPixels());
            photo.Apply();
            //Encode to a PNG
            byte[] bytes = photo.EncodeToPNG();
            //Write out the PNG. Of course you have to substitute your_path for something sensible
            File.WriteAllBytes(imagePath, bytes);
        }
        else{
            Debug.Log("Empty face");
        }
    }*/
    private readonly Size requiredSize = new Size(128, 128);
    private FaceRecognizer recognizer;
    public UnityEngine.TextAsset recognizerXml; // Attach your pretrained xml to this in inspector mode
    // Change the arrays below if you have more entries, or just use a database to connect to this application for a large scale project
    private string[] names = {"allen", "other"};
    private string[] ids = {"12345", "N/A"};
    private string[] levels = {"13", "N/A"};
    private string[] topPokemon = {"Lugia", "N/A"};
    private string [] pokemonLevels = {"45", "N/A"};
    void inferFace(Mat image){
        recognizer = FaceRecognizer.CreateFisherFaceRecognizer(); // Use specific recognizer
        recognizer.Load(new FileStorage(recognizerXml.text, FileStorage.Mode.Read | FileStorage.Mode.Memory)); 
        var gray = image.CvtColor(ColorConversionCodes.BGR2GRAY);
		Cv2.EqualizeHist(gray, gray);
        // detect matching regions (faces bounding)
		OpenCvSharp.Rect[] rawFaces = cascade.DetectMultiScale(gray, 1.1, 6);
        if (rawFaces.Length == 0){
            faceName.text = "No one detected";
            return;
        }
        foreach (var faceRect in rawFaces)
		{
            var grayFace = new Mat(gray, faceRect);
			if (requiredSize.Width > 0 && requiredSize.Height > 0)
				grayFace = grayFace.Resize(requiredSize);
            int label = -1;
            // Confidence is the incorrect term, the lower the value for confidence the actual more confidence we have that the label is correct
            // In deep learning we have energy functions that reduces distance of positive examples and increases distance of negative examples
            // In this case, the confidence is simply the distance of the inferred logits from the actual label output
            // Therefore, the lower the confidence, the higher we are confident that we inferred correctly.
			double confidence = 0.0;
			recognizer.Predict(grayFace, out label, out confidence);
            if(label == names.Length - 1){
                showModel = false;
            }
            else{
                showModel = true;
            }
            faceName.text = names[label];
            trainerInfo.text = "Trainer name: " + names[label]
            + "\nTrainer id: " + ids[label] + 
            "\nTrainer level: " + levels[label] + 
            "\nTop pokemon: " + topPokemon[label] + 
            "\nPokemon level: " + pokemonLevels[label];
            //Debug.Log("Labeled as: " + names[label] + " with confidence: " + confidence);
        }
    }
    // Below is the way to train the recognizer
    // As a note, you will need to make folders for the subject labels you want to train
    // Additionally, if the number of faces you want to train is a small number, you will require a great amount of negative examples
    // You can search or google for datasets or create your own datasets with faces of random people and name this folder as "unknown" or something sensible
    /*void Start()
		{
			string [] roots = {UnityEngine.Application.dataPath, "Images"}; // Use this path or any folder you want as a root for your training
        	string root = Path.Combine(roots);
			TrainRecognizer(root);
		}
		private void TrainRecognizer(string root)
		{
			// This one was actually used to train the recognizer. I didn't push much effort and satisfied once it
			// distinguished own face on a sample image, for the real-world application you might want to
			// refer to the following documentation:
			// OpenCV documentation and samples: http://docs.opencv.org/3.0-beta/modules/face/doc/facerec/tutorial/facerec_video_recognition.html
			// Training sets overview: https://www.kairos.com/blog/60-facial-recognition-databases
			// Another OpenCV doc: http://docs.opencv.org/2.4/modules/contrib/doc/facerec/facerec_tutorial.html#face-database
			UnityEngine.Debug.Log("Start traversing images.");
			int id = 0;
			var ids = new List<int>();
			var mats = new List<Mat>();
			var namesList = new List<string>();

            // Note that in this implementation, the folder name is the label name and the images within each folder are trained
			// Personally, I suggest at least 20 images of each subjet for a small number of labels and 20 images of "unknown" random dataset
            // This way, you can differentiate from people in our dataset and people who are not in our dataset and solely used for "negative examples"
			foreach (string dir in Directory.GetDirectories(root))
			{
				UnityEngine.Debug.Log("Current directory" + dir);
				string name = System.IO.Path.GetFileNameWithoutExtension(dir);
				if (name.StartsWith("-"))
					continue;
				namesList.Add(name); // Unique names list per folder
				UnityEngine.Debug.LogFormat("{0} = {1}", id, name);

				foreach (string file in Directory.GetFiles(dir))
				{
					var bytes = File.ReadAllBytes(file);
					var texture = new UnityEngine.Texture2D(2, 2);
					texture.LoadImage(bytes);

					ids.Add(id);

					// each loaded texture is converted to OpenCV Mat, turned to grayscale (assuming we have RGB source) and resized
					var mat = OpenCvSharp.Unity.TextureToMat(texture);
					mat = mat.CvtColor(ColorConversionCodes.BGR2GRAY);
					if (requiredSize.Width > 0 && requiredSize.Height > 0)
						mat = mat.Resize(requiredSize);
					mats.Add(mat);
				}
				id++;
			}
			UnityEngine.Debug.Log("Finished traversing images.");
			names = namesList.ToArray();

			// train recognizer and save result for the future re-use, while this isn't quite necessary on small training sets, on a bigger set it should
			// give serious performance boost
			UnityEngine.Debug.Log("Start training images.");
			//for (int i = 0; i < 20; i++){
				recognizer.Train(mats, ids);
			//}
			UnityEngine.Debug.Log("Ids available: " + id);
			recognizer.Save(root + "/face-recognizer.xml");
			UnityEngine.Debug.Log("Images finished training");
			UnityEngine.Debug.Log("Current root directory: " + root);
		}
    }*/
}
