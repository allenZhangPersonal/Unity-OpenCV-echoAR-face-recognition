# Unity-OpenCV-echoAR-face-recognition
Simple face detection+recognition demo utilizing Unity, openCV, and echoAR implemented in Unity 2019.4.26f1

## Register
Don't have an API key? Make sure to register for FREE at [echoAR](https://console.echoar.xyz/#/auth/register).

## Setup
* Create a new Unity project.
* Clone the current repository sample code.
* Open the 'faceRecognizer' sample scene under `Assets\OpenCV+Unity\Demo\Face_Recognizer`.
* [Set the API key](https://docs.echoar.xyz/unity/using-the-sdk) in the `echoAR.cs` script inside the `Assets\echoAR\echoAR.prefab` using the Inspector.
* [Add the model](https://docs.echoar.xyz/quickstart/add-a-3d-model) from the [videos](/Models) folder to the console.
* [Add all the metadata](https://docs.echoar.xyz/web-console/manage-pages/data-page/how-to-add-data#adding-metadata) listed in the [metadata](/Metadata) folder.

## Build & Run
* [Build and run the AR application](https://docs.echoar.xyz/unity/adding-ar-capabilities#4-build-and-run-the-ar-application). Verify that the `Assets\OpenCV+Unity\Demo\Face_Recognizer\Face_Recognizer` scene is ticked in the `Scenes in Build` list and click `Build And Run`.

## Training own face recognizer

Open `Assets\camDetector.cs` script and read the commented notes at the bottom of the script for function definition: private void TrainRecognizer(string root). The training function will save the recognizer .xml under [images](/Assets/Images) folder. Each folder under the [images](/Assets/Images) folder will possess the label name of a specific subject that will be trained. It is suggested to have your positive subject names as the folder titles as well as an "unknown" folder specifically containing images of random people to train as negative examples. For more information on training, please consult the following documents.

OpenCV documentation and samples: http://docs.opencv.org/3.0-beta/modules/face/doc/facerec/tutorial/facerec_video_recognition.html

Training sets overview: https://www.kairos.com/blog/60-facial-recognition-databases

Another OpenCV doc: http://docs.opencv.org/2.4/modules/contrib/doc/facerec/facerec_tutorial.html#face-database


## Learn more
Refer to our [documentation](https://docs.echoar.xyz/unity/) to learn more about how to use Unity and echoAR.

## Support
Feel free to reach out at [support@echoAR.xyz](mailto:support@echoAR.xyz) or join our [support channel on Slack](https://join.slack.com/t/echoar/shared_invite/enQtNTg4NjI5NjM3OTc1LWU1M2M2MTNlNTM3NGY1YTUxYmY3ZDNjNTc3YjA5M2QyNGZiOTgzMjVmZWZmZmFjNGJjYTcxZjhhNzk3YjNhNjE). 

## Screenshots
Demo

![face_recognition](https://user-images.githubusercontent.com/85501187/121783900-5c044180-cb7f-11eb-81d5-031f75122072.gif)

Videos
![models](https://user-images.githubusercontent.com/85501187/121057692-8a53dc80-c78d-11eb-8e87-982a5307d2b3.JPG)

Metadata
![metadata](https://user-images.githubusercontent.com/85501187/121057656-7c9e5700-c78d-11eb-8dac-b3bac4d7826f.JPG)

Demo created by [Allen Zhang](https://github.com/allenZhangPersonal)
