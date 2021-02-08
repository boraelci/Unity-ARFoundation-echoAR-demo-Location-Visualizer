/**************************************************************************
* Copyright (C) echoAR, Inc. 2018-2020.                                   *
* echoAR, Inc. proprietary and confidential.                              *
*                                                                         *
* Use subject to the terms of the Terms of Service available at           *
* https://www.echoar.xyz/terms, or another agreement                      *
* between echoAR, Inc. and you, your company or other organization.       *
***************************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

public class CustomBehaviour : MonoBehaviour
{
    [HideInInspector]
    public Entry entry;

    /// <summary>
    /// EXAMPLE BEHAVIOUR
    /// Queries the database and names the object based on the result.
    /// </summary>

    // Use this for initialization
    void Start()
    {
        string APIKey = "<API_KEY>";
        // Add RemoteTransformations script to object and set its entry
        this.gameObject.AddComponent<RemoteTransformations>().entry = entry;

        // Qurey additional data to get the name
        string value = "";
        if (entry.getAdditionalData() != null && entry.getAdditionalData().TryGetValue("name", out value))
        {
            // Set name
            this.gameObject.name = value;

            if (this.gameObject.name == "PIN") {
              string url = "https://console.echoAR.xyz";
              string key = APIKey;
              string pin_entryID = entry.getId();
              string latitude = "";
              string longitude = "";
              entry.getAdditionalData().TryGetValue("latitude", out latitude);
              entry.getAdditionalData().TryGetValue("longitude", out longitude);
              LocationVisualize(url, key, pin_entryID, latitude, longitude);
            }
        }
    }

    void LocationVisualize(string url, string key, string pin_entryID, string latitude, string longitude){

      string[] GPS_names = new string[2];
      GPS_names[0] = "latitude";
      GPS_names[1] = "longitude";

      string[] GPS_values = new string[2];
      GPS_values[0] = latitude;
      GPS_values[1] = longitude;

      string[] coordinate_names = new string[2];
      coordinate_names[0] = "x";
      coordinate_names[1] = "z";

      string[] coordinate_values = new string[2];

      ProcessCoordinates(url, key, pin_entryID, GPS_names, GPS_values, coordinate_names, coordinate_values);

    }

    void ProcessCoordinates(string url, string key, string pin_entryID, string[] GPS_names, string[] GPS_values, string[] coordinate_names, string[] coordinate_values){
      var latitude = Convert.ToDouble(GPS_values[0]);
      var longitude = Convert.ToDouble(GPS_values[1]);

      var origin_latitude = 40.807861;
      var origin_longitude = -73.962112;
      var cosa = 0.039968038;
      var sina = 0.999200959;
      var scale = 0.016433786;
      var xcord = ((longitude-origin_longitude)/scale*sina)-((latitude-origin_latitude)/scale*cosa);
      var zcord = ((latitude-origin_latitude)/scale*sina)+((longitude-origin_longitude)/scale*cosa);

      coordinate_values[0] = xcord.ToString();
      coordinate_values[1] = zcord.ToString();
      UpdatePinCoordinates(url, key, pin_entryID, coordinate_names, coordinate_values);
    }

    void UpdatePinCoordinates(string url, string key, string pin_entryID, string[] coordinate_names, string[] coordinate_values) {
      for (int i = 0; i < coordinate_names.Length; i++)
      {
        StartCoroutine(UploadEntryMetadata(url,key,pin_entryID,coordinate_names[i],coordinate_values[i]));
      }
    }

    IEnumerator UploadEntryMetadata(string url, string key, string entryID, string data, string valueIN) {
      WWWForm form = new WWWForm();
      form.AddField("key", key);
      form.AddField("entry", entryID);
      form.AddField("data", data);
      form.AddField("value", valueIN);

      using (UnityWebRequest www = UnityWebRequest.Post(url+"/post?", form))
      {
          yield return www.SendWebRequest();

          if (www.isNetworkError || www.isHttpError)
          {
              Debug.Log(www.error);
          }
          else
          {
              Debug.Log("Form upload complete!");
          }
      }
    }

    // Update is called once per frame
    void Update()
    {

    }

}
