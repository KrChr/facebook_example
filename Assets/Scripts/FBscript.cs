using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Facebook;
using Facebook.Unity;


public class FBscript : MonoBehaviour {

	public GameObject DialogLoggedIn;
	public GameObject DialogLoggedOut;
	public GameObject DialogUsername;
	public GameObject DialogID;
	public GameObject DialogEmail;
	public GameObject DialogLocation;
	public GameObject DialogCurrency;
	public GameObject DialogCurrencyUSD;
	public GameObject DialogProfilePic;

	public GameObject FriendsScrollView;
	public GameObject FriendPrefab;

	public string FBFriendsCallback;

	public Texture2D textFb2, UserImg;

	[SerializeField]
	private List<string> permissions;



	void Awake ()
	{
		FB.Init (SetInit, OnHideUnity);
	}


	void SetInit()
	{
		if (FB.IsLoggedIn) {
			Debug.Log ("FB is logged in");

		} else 
		{
			Debug.Log ("FB is not logged in");
		}

		DealWithFBMenus (FB.IsLoggedIn);
	}


	void OnHideUnity(bool isGameShown)
	{
		if (!isGameShown) {
			Time.timeScale = 0;
		} else 
		{
			Time.timeScale = 1;
		}
	}


	public void FBlogin()
	{	
		//Get Token
		//https://developers.facebook.com/tools/explorer

		//https://developers.facebook.com/docs/facebook-login/permissions
		permissions = new List<string> ();
		permissions.Add ("public_profile");
		permissions.Add ("email");
		permissions.Add ("user_friends");
		permissions.Add ("user_hometown");
		permissions.Add ("user_location");

		FB.LogInWithReadPermissions (permissions, AuthCallBack);

		Debug.Log ("FB.LogInWithReadPermissions done");
	}


	void AuthCallBack(IResult result)
	{
		if (result.Error != null) 
		{
			Debug.Log (result.Error);
		} else 
		{
			if (FB.IsLoggedIn) 
			{
				Debug.Log ("FB is logged in");

				// AccessToken class will have session details
				var aToken = Facebook.Unity.AccessToken.CurrentAccessToken;

				// Print current access token's User ID
				Debug.Log("UserID = " + aToken.UserId);

				// Print current access token's granted permissions
				foreach (string perm in aToken.Permissions) 
				{
					Debug.Log ("AuthCallBack, permissions: " + perm);
				}
			}
			else 
			{
				Debug.Log ("FB is not logged in");
			}

			DealWithFBMenus (FB.IsLoggedIn);
		}
	}


	void DealWithFBMenus(bool isLoggedIn)
	{
		if (isLoggedIn) 
		{
			DialogLoggedIn.SetActive (true);
			DialogLoggedOut.SetActive (false);

			// https://developers.facebook.com/docs/graph-api/reference/v2.5/user
			FB.API ("/me/friends?fields=name", HttpMethod.GET, DisplayFriendsName);
			FB.API ("/me?fields=name", HttpMethod.GET, DisplayUserName);
			FB.API ("/me?fields=email", HttpMethod.GET, DisplayUserEmail);
			FB.API ("/me?fields=id", HttpMethod.GET, DisplayID);
			FB.API ("/me?fields=location", HttpMethod.GET, DisplayLocationName);
			FB.API ("/me?fields=currency", HttpMethod.GET, DisplayCurrency);
			FB.API ("/me?fields=currency", HttpMethod.GET, DisplayCurrencyUSD);
			FB.API ("/me/picture?type=square&height=128&width=128", HttpMethod.GET, DisplayProfilePic);
		} else 
		{
			DialogLoggedIn.SetActive (false);
			DialogLoggedOut.SetActive (true);
		}
	}

/*
		to do:
			1. id = /me?fields=id
			2. imię = /me?fields=first_name
			3. nazwisko = /me?fields=last_name
			4. avatar = /me/picture?type=square&height=128&width=128 (IGraphResult)
			5. miasto = /me?fields=location[name] (dictionary)
			
			6. lista znajomych = /me/friends?fields=name
			The field 'friendlists' is only accessible on the User object after the user grants the 'read_custom_friendlists' permission.

			Only friends who installed this app are returned in API v2.0 and higher. total_count in summary represents the total number of friends,
		    including those who haven't installed the app

				6.1	imię
				6.2	nazwisko
				6.3	avator

			https://developers.facebook.com/docs/games/services/gamerequests#invitablefriends
			7. lista znajomych do zaproszenia = me/invitable_friends?fields (requires the user_friends permission)
				7.1	imię
				7.2	nazwisko
				7.3	avatar
				7. preferowaną dla gracza walutę = me?fields=currency[user_currency]  (dictionary)
				8. kurs dolara w stosunku do preferowanej przez usera waluty = me?fields=currency[usd_exchange_inverse]  (dictionary)
*/

	void DisplayUserName(IResult result)
	{
		Text Username = DialogUsername.GetComponent<Text> ();

		if (result.Error == null) 
		{
			foreach (var item in result.ResultDictionary)
			{
//				Debug.Log ("DisplayUserName, result.ResultDictionary = " + item + ", ");
			}
			Username.text = "Name: " + result.ResultDictionary ["name"] as string;
		} else 
		{
			Debug.Log (result.Error);
		}
	}


	void DisplayUserEmail(IResult result)
	{
		Text Username = DialogEmail.GetComponent<Text> ();

		if (result.Error == null) 
		{
			foreach (var item in result.ResultDictionary) 
			{
//				Debug.Log ("DisplayUserEmail, result.ResultDictionary = " + item + ", ");
			}	
			Username.text = "e-mail: " + result.ResultDictionary ["email"] as string;
		} else 
		{
			Debug.Log (result.Error);
		}
	}


	void DisplayFriendsName(IResult result)
	{
/*
	Only friends who installed this app are returned in API v2.0 and higher.
	total_count in summary represents the total number of friends,
	including those who haven't installed the app
*/
		if (result.Error == null) 
		{
			foreach (var item in result.ResultDictionary) 
			{
//				Debug.Log ("DisplayFriendsName, result.ResultDictionary = " + item + ", ");
			}	

			var resultObj = new List<object>();
			resultObj = (List<object>)(result.ResultDictionary ["data"]);

			foreach (var item in resultObj) 
			{
//				Debug.Log ("DisplayFriendsName, Object: " + item);

				Dictionary<string,object> itemDictionary = item as Dictionary<string,object>;
//				Debug.Log ("DisplayFriendsName, itemDictionary [id] = " + itemDictionary ["id"] + ", [name] = " + itemDictionary ["name"]);

				StartCoroutine(UserImageList (itemDictionary));
			}
		} else 
		{
			Debug.Log (result.Error);
		}
 	}


	void DisplayID(IResult result)
	{
		Text ID = DialogID.GetComponent<Text> ();

		if (result.Error == null) 
		{
			foreach (var item in result.ResultDictionary) 
					{
//					Debug.Log ("DisplayID, result.ResultDictionary = " + item + ", ");
					}	
			ID.text = "ID: " + result.ResultDictionary ["id"] as string;//first_name
		} else 
		{
				Debug.Log (result.Error);
		}
	}		


	void DisplayLocationName(IResult result)
	{
		Text Location = DialogLocation.GetComponent<Text> ();

		if (result.Error == null) 
		{
			foreach (var item in result.ResultDictionary) 
			{
//				Debug.Log ("DisplayLocationName, result.ResultDictionary = " + item + ", ");
			}	
			//Location.text = "Location: " + result.ResultDictionary ["location"] as string;//first_name

			Dictionary<string,object> location = result.ResultDictionary ["location"] as Dictionary<string,object>;

			Location.text = "Location: " + location ["name"] as string;//first_name
//			Debug.Log ("DisplayLocationName, result.ResultDictionary, location [name] = " + location ["name"]); 
		} else 
		{
			Debug.Log (result.Error);
		}
	}



	void DisplayCurrency(IResult result)
	{
		Text currency = DialogCurrency.GetComponent<Text> ();

		if (result.Error == null) 
		{
			foreach (var item in result.ResultDictionary) 
			{
//				Debug.Log ("DisplayCurrency, result.ResultDictionary = " + item + ", ");
			}	
			

			Dictionary<string,object> currencyName = result.ResultDictionary ["currency"] as Dictionary<string,object>; //Facebook.MiniJSON.Json.Deserialize (result.RawResult) as Dictionary<string,object>;

			currency.text = "Currency: " + currencyName ["user_currency"] as string;//first_name
//			Debug.Log ("DisplayCurrency, result.ResultDictionary, currencyName [user_currency] = " + currencyName ["user_currency"]);
		} else 
		{
			Debug.Log (result.Error);
		}
	}	


	void DisplayCurrencyUSD(IResult result)
	{
		Text currencyUSD = DialogCurrencyUSD.GetComponent<Text> ();

		if (result.Error == null) 
		{
			foreach (var item in result.ResultDictionary) 
			{
//				Debug.Log ("DisplayCurrencyUSD, result.ResultDictionary = " + item + ", ");
			}	

			Dictionary<string,object> currencyUSDValue = result.ResultDictionary ["currency"] as Dictionary<string,object>;

			currencyUSD.text = "1 USD = " + currencyUSDValue ["usd_exchange_inverse"] as string;
//			Debug.Log ("usd_exchange_inverse =  " + currencyUSDValue ["usd_exchange_inverse"]);
		} else 
		{
			Debug.Log (result.Error);
		}
	}	


	IEnumerator UserImageList(Dictionary<string,object> myItemDictionary)
	{
		GameObject myNewFriend = Instantiate (FriendPrefab, new Vector3(transform.position.x, transform.position.y, transform.position.z), Quaternion.identity) as GameObject;
		myNewFriend.transform.parent = FriendsScrollView.transform;
		
		Text FriendName = myNewFriend.transform.Find("Name").GetComponent<Text> ();
		Image ProfilePic = myNewFriend.transform.Find("Image").GetComponent<Image> ();


		FriendName.text = "" + myItemDictionary ["name"];
		
//		Debug.Log ("UserImageList, myItemDictionary [id] = " + myItemDictionary ["id"] + ", [name] = " + myItemDictionary ["name"]);
		
//		Debug.Log ("UserImageList, UserFriendsImage start, ID = " + myItemDictionary ["id"]);
		WWW url = new WWW("https" + "://graph.facebook.com/" +myItemDictionary ["id"] + "/picture?type=large"); 
		Texture2D textFb2 = new Texture2D(128, 128, TextureFormat.DXT1, false); //TextureFormat must be DXT5
		yield return url;
			url.LoadImageIntoTexture(textFb2);


			ProfilePic.sprite = Sprite.Create (textFb2, new Rect (0, 0, textFb2.width, textFb2.height), new Vector2 ());
			UserImg = textFb2;
			Debug.Log ("UserFriendsImage done");
	}


	void DisplayProfilePic(IGraphResult result)
	{
		if (result.Texture != null) 
		{

			Image ProfilePic = DialogProfilePic.GetComponent<Image> ();
			ProfilePic.sprite = Sprite.Create (result.Texture, new Rect (0, 0, 128, 128), new Vector2 ());
		}
	}
}
