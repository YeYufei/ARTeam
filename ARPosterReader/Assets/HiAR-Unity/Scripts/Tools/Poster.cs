﻿using System.Collections.Generic;
using UnityEngine.Networking;
using System.Collections;
using UnityEditor;
using UnityEngine;
using Proyecto26;
using System.Net;
using System.IO;
using System;

namespace Models {
	[Serializable]
	public class Poster {
		public string KeyGroup;
		public string KeyId;
		public string Detail;
		public string Url;

		public void GetPoster() {
			Get ("/posters");
		}

		public void SavePoster() {
			Put ("/posters");
		}

		// RESTful, HTTP verb: GET
		public string Get(string endpoint) {
			var uri = Tools.Server + endpoint + "/" + KeyGroup + "/" + KeyId + "/" + Detail + "/" + Url;
			RestClient.Get<PostersResponse> (new RequestHelper {
				Uri = uri
			}).Then(res => {
				this.Detail = res.poster.Detail;
				this.Url = res.poster.Url;
				return res.error;
			});
		}

		public string Put(string endpoint) {
			var uri = Tools.Server + endpoint;
			RestClient.Put<PostersResponse>(new RequestHelper {
				Uri = uri,
				BodyString = new Tools().MakeJsonStringFromClass<Poster>(this),
				EnableDebug = true
			}).Then(res => {
				//if(!string.Equals(res.error, "")) {
					//EditorUtility.DisplayDialog ("Error", res.error, "Ok");
				//} else {
					//EditorUtility.DisplayDialog ("Success", res.success.ToString(), "Ok");
				//}
				return res.error;
			});
		}

		[Serializable]
		public class PostersResponse {
			public string error;
			public bool isexist;
			public bool success;

			public Poster poster;
		}
	}
}
