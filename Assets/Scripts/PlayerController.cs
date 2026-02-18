using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public CharacterController controller;
    public Transform cam;

    public Material skyboxMaterial;
    public Light sunLight;
    public Light[] spotLights;
    public Light[] pointLights;

    public TextMeshProUGUI modetext;

    public InputActionReference move;
    public InputActionReference look;
    public InputActionReference toggler;

    bool isNight = false;
    Coroutine transitionRoutine;

    float speed = 5f;
    Vector2 movedirection;

    float sens = 0.1f;
    float xRotation = 0f;

    private void Start()
    {
        modetext.text = isNight ? "Night Mode" : "Day Mode";
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnEnable()
    {
        toggler.action.Enable();
        toggler.action.performed += ctx => ToggleTime();
    }

    private void OnDisable()
    {
        toggler.action.Disable();
        toggler.action.performed -= ctx => ToggleTime();
    }

    void ToggleTime()
    {
        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
        }
        isNight = !isNight;
        modetext.text = isNight ? "Night Mode" : "Day Mode";
        transitionRoutine = StartCoroutine(Transition(isNight? 1f : 0f));
    }

    IEnumerator Transition(float target)
    {
        float time = 0f;
        float duration = 1.5f;

        float startSunSize = skyboxMaterial.GetFloat("_SunSize");
        float startSunConv = skyboxMaterial.GetFloat("_SunSizeConvergence");
        float startAtmos = skyboxMaterial.GetFloat("_AtmosphereThickness");
        Color startSkyTint = skyboxMaterial.GetColor("_SkyTint");
        float startExpos = skyboxMaterial.GetFloat("_Exposure");
        float startSunIntensity = sunLight.intensity;
        float startSpotIntensity = spotLights.Length > 0 ? spotLights[0].intensity : 0f;
        float startPointIntensity = pointLights.Length > 0 ? pointLights[0].intensity : 0f;

        float targetSunSize = isNight ? 0f : 0.07f;
        float targetSunConv = isNight ? 0f : 4.5f;
        float targetAtmos = isNight ? 0.2f : 1f;
        Color targetSkyTint = isNight ? Color.black : Color.grey;
        float targetExpos = isNight ? 0.08f : 1.3f;
        float targetSunIntensity = isNight ? 0f : 7.5f;
        float targetSpotIntensity = isNight ? 4f : 0f;
        float targetPointIntensity = isNight ? 1f : 0f;

        while (time < duration)
        {
            float t = time / duration;
            t = Mathf.SmoothStep(0f, 1f, t);

            //Skybox Changing
            skyboxMaterial.SetFloat("_SunSize", Mathf.Lerp(startSunSize, targetSunSize, t));
            skyboxMaterial.SetFloat("_SunSizeConvergence", Mathf.Lerp(startSunConv, targetSunConv, t));
            skyboxMaterial.SetFloat("_AtmosphereThickness", Mathf.Lerp(startAtmos, targetAtmos, t));
            skyboxMaterial.SetColor("_SkyTint", Color.Lerp(Color.white, targetSkyTint, t));
            skyboxMaterial.SetFloat("_Exposure", Mathf.Lerp(startExpos, targetExpos, t));

            //Light Changing
            sunLight.intensity = Mathf.Lerp(startSunIntensity, targetSunIntensity, t);
            foreach (var light in spotLights)
            {
                light.intensity = Mathf.Lerp(startSpotIntensity, targetSpotIntensity, t);
            }
            foreach (var light in pointLights)
            {
                light.intensity = Mathf.Lerp(startPointIntensity, targetPointIntensity, t);
            }

            time += Time.deltaTime;
            yield return null;
        }
    }

    private void Update()
    {
        movedirection = move.action.ReadValue<Vector2>();

        Vector2 lookinput = look.action.ReadValue<Vector2>();

        float mouseX = lookinput.x * sens;
        float mouseY = lookinput.y * sens;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cam.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    private void FixedUpdate()
    {
        Vector3 moveinput = transform.right * movedirection.x + transform.forward * movedirection.y;
        if (controller != null)
            controller.Move(moveinput * speed * Time.fixedDeltaTime);
    }
}