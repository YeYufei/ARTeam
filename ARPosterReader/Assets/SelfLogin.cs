﻿using UnityEngine;
using UnityEngine.UI;
using Models;
using UnityEngine.SceneManagement;
using System.IO;
using System;

public static class storeLoginSessionId{
    public static int loginId = -1;
    public static string name = null;
    public static string email = "";
}

public class SelfLogin : MonoBehaviour {


	public InputField _email;
	public InputField _password;


	public void checkPassword()
	{
		User u = new User ();
		u.username = _email.text;
		u.password = _password.text;
		u.Login();
        if (u.authenticated)
        {
			string path = Application.persistentDataPath +"/User.txt";
            StreamWriter writer;
            string uID = u.id.ToString();
            storeLoginSessionId.loginId = u.id;
            if (string.IsNullOrEmpty(u.name))
            {
                storeLoginSessionId.name = u.username;
            }
            else
            {
                storeLoginSessionId.name = u.name;
            }

            storeLoginSessionId.email = u.email;

            if (!File.Exists(path))
            {
                writer = File.CreateText(path);
                writer.Write(uID+"\n");
                writer.Write(storeLoginSessionId.name + "\n");
                writer.Close();
            }
            else
            {
                File.WriteAllText(path, uID + "\n" + storeLoginSessionId.name + "\n");
            }
            
            SceneManager.LoadScene("HiARRobot");
        }
        else
        {
            Debug.Log("Login Failed");
        }
	}

	public void saveUserInFile(int userID){
		
	}

}
