using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class GUI : MonoBehaviour {


    private Dictionary<string, object> parameters = new Dictionary<string, object>();


    //Значения по умолчанию для полей GUI
	void Awake()
    {
        parameters.Add("chunkSize", 3);
        parameters.Add("sample", "Normal");
        parameters.Add("roughness", 0f);
        parameters.Add("plato", true);
    }


    //Запуск события инициализации параметров и старта генерации чанка
    public void OnGenerateButton()
    {
        EventManager.InitParameters(parameters);
    }



    //--------------------Обработка полей GUI------------------------------------
    public void OnExitButton()
    {
        Application.Quit();
    }

    public void OnChunkSizeChange (Dropdown dropdown)
    {
        parameters["chunkSize"] = int.Parse(dropdown.captionText.text);
    }

    public void OnSampleChange (Dropdown dropdown)
    {
        parameters["sample"] = dropdown.captionText.text;
    }

    public void OnRoughnessChange(Slider slider)
    {
        Transform text;
        text = slider.transform.Find("Text");
        text.GetComponent<Text>().text = slider.value.ToString();

        parameters["roughness"] = slider.value;
    }

    public void OnPlatoChange(Toggle toggle)
    {
        parameters["plato"] = toggle.isOn;
    }
}
