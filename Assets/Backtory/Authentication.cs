using System.Collections.Generic;
using Pegah.SaaS.Utility;
using System;
using UnityEngine;

public class Authentication {
	public static string ROOT_URL = "https://api.backtory.com/auth";
	// public static string ROOT_URL = "http://api1.backtory.com/auth";
	string authenticationId;
	string authenticationKey;
	
	public Authentication(string authenticationId, string authenticationKey) {
		this.authenticationId = authenticationId;
		this.authenticationKey = authenticationKey;
	}
	
	public void login(string username, string password
		, Action<LoginResponse> onSuccess, Action<int> onFailed = null) {
		string url = ROOT_URL + "/login";

		Dictionary<string, string> customHeaders = new Dictionary<string, string> ();
		Dictionary<string, object> pathParams = new Dictionary<string, object> ();
		object postData = "";

		customHeaders.Add("X-Backtory-Authentication-Id", authenticationId);
		customHeaders.Add("X-Backtory-Authentication-Key", authenticationKey);

		HttpHelper.sendRequest<LoginResponse> ("post",
		 url + "?username=" + username + "&password=" + password
		 , pathParams, postData, customHeaders,
                (_object) => {
					if (onSuccess != null)
                   		onSuccess(_object);
                },
                (x) =>  {
					if (onFailed != null)
                   		onFailed(x);
                }
            );
	}
	
		public void register(string username, string password, string firstName, string lastName, 
		Action<RegisterResponse> onSuccess, Action<int> onFailed = null) {
		string url = ROOT_URL + "/users";

		RegisterRequest registerRequest = new RegisterRequest();
		registerRequest.username = username;
		registerRequest.password = password;
		registerRequest.firstName = firstName;
		registerRequest.lastName = lastName;

		Dictionary<string, string> customHeaders = new Dictionary<string, string> ();
		Dictionary<string, object> pathParams = new Dictionary<string, object> ();
		
		customHeaders.Add("X-Backtory-Authentication-Id", authenticationId);

		HttpHelper.sendRequest<RegisterResponse> ("post",
		 url, pathParams, registerRequest, customHeaders,
                (_object) => {
					if (onSuccess != null)
                   		onSuccess(_object);
                },
                (x) =>  {
					if (onFailed != null)
                   		onFailed(x);
                }
            );
	}
}
