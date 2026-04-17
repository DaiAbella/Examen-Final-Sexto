using UnityEngine;
using PlayFab;
using TMPro;
using PlayFab.ClientModels;
using System.Threading.Tasks;
using System;


public class PlayfabManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private TMP_InputField colorInput;

    [Header("Player Data")]
    [SerializeField] private string playerClass;
    [SerializeField] private int level;
    [SerializeField] private float health;

    [Header("Menus")]
    [SerializeField] private GameObject loginMenu;
    [SerializeField] private GameObject gameMenu;

    // 🌐 VARIABLE GLOBAL (CLAVE)
    public static string SavedWeaponColor = "white";

    private void Start()
    {
        if (PlayFabSettings.TitleId == null)
        {
            PlayFabSettings.TitleId = "1E6FB1";
        }

        if (loginMenu != null) loginMenu.SetActive(true);
        if (gameMenu != null) gameMenu.SetActive(false);
    }

    void ShowGameMenu()
    {
        if (loginMenu != null) loginMenu.SetActive(false);
        if (gameMenu != null) gameMenu.SetActive(true);
    }

    // =========================
    // REGISTER
    // =========================

    public async void RegisterUser()
    {
        try
        {
            await RegisterPlayfabAccount();
            Debug.Log("Usuario Registrado");
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public async Task<RegisterPlayFabUserResult> RegisterPlayfabAccount()
    {
        var tcs = new TaskCompletionSource<RegisterPlayFabUserResult>();

        var request = new RegisterPlayFabUserRequest()
        {
            Username = usernameInput.text.ToLower(),
            DisplayName = usernameInput.text,
            Email = usernameInput.text + "@test.com",
            Password = passwordInput.text,
        };

        PlayFabClientAPI.RegisterPlayFabUser(
            request,
            r => tcs.SetResult(r),
            e => tcs.SetException(new Exception(e.GenerateErrorReport()))
        );

        return await tcs.Task;
    }

    // =========================
    // LOGIN
    // =========================

    public async void PlayfabLogin()
    {
        try
        {
            await LoginWithPlayfab();
            Debug.Log("Sesion iniciada");

            var data = await RequestPlayerData();

            if (data.Data != null && data.Data.ContainsKey("WeaponColor"))
            {
                SavedWeaponColor = data.Data["WeaponColor"].Value;
                Debug.Log("Color cargado: " + SavedWeaponColor);
            }

            ShowGameMenu();
        }
        catch (Exception error)
        {
            Debug.Log(error.Message);
        }
    }

    public async Task<LoginResult> LoginWithPlayfab()
    {
        var tcs = new TaskCompletionSource<LoginResult>();

        var request = new LoginWithPlayFabRequest()
        {
            Username = usernameInput.text.ToLower(),
            Password = passwordInput.text,
        };

        PlayFabClientAPI.LoginWithPlayFab(
            request,
            r => tcs.SetResult(r),
            e => tcs.SetException(new Exception(e.GenerateErrorReport()))
        );

        return await tcs.Task;
    }

    // =========================
    // UPDATE DATA (GUARDAR COLOR)
    // =========================

    public async void UploadPlayerData()
    {
        try
        {
            await SetPlayerData();
            Debug.Log("Datos subidos a PlayFab");
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public async Task<UpdateUserDataResult> SetPlayerData()
    {
        var tcs = new TaskCompletionSource<UpdateUserDataResult>();

        var request = new UpdateUserDataRequest()
        {
            Data = new System.Collections.Generic.Dictionary<string, string>()
            {
                {"PlayerClass", playerClass},
                {"Level", level.ToString()},
                {"Health", health.ToString()},
                {"WeaponColor", colorInput.text.ToLower()} // 👈 CLAVE
            }
        };

        PlayFabClientAPI.UpdateUserData(
            request,
            r => tcs.SetResult(r),
            e => tcs.SetException(new Exception(e.GenerateErrorReport()))
        );

        return await tcs.Task;
    }

    // =========================
    // GET DATA
    // =========================

    public async Task<GetUserDataResult> RequestPlayerData()
    {
        var tcs = new TaskCompletionSource<GetUserDataResult>();

        PlayFabClientAPI.GetUserData(
            new GetUserDataRequest(),
            r => tcs.SetResult(r),
            e => tcs.SetException(new Exception(e.GenerateErrorReport()))
        );

        return await tcs.Task;
    }
}