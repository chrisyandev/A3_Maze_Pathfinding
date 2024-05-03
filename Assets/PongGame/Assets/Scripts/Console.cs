using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;


public class Console : MonoBehaviour
{

    // if this is over-engineered, i'll change to switch-case.
    private Dictionary<string, Action<string>> cmdList; 

    private string buffer;

    // lots of references below.
    // can easily just learn the hierachy
    // and store the parent console, shud i?
    [SerializeField]
    private GameObject m_consoleCanvas;  
    [SerializeField]
    private TMP_InputField m_cmdLine;

    [SerializeField]
    private TMP_Text m_cmdHistory;

    [SerializeField]
    private GameObject m_background; 

    private InputActions m_inputActions;
    private InputAction m_toggleConsoleAction;

    private void Awake() {
        m_cmdLine.onEndEdit.AddListener(processCmd);
        buffer = "";
        cmdList = new Dictionary<string, Action<string>>()
        {
            ["colour"] = changeBGColour,
            ["setBallSpd"] = setBallSpeed
        };
        m_inputActions = new InputActions();
        m_toggleConsoleAction = m_inputActions.Console.ToggleConsole;

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        if (!m_cmdLine.isFocused && m_toggleConsoleAction.triggered)
        {
            m_consoleCanvas.SetActive( !m_consoleCanvas.activeSelf );
            if (m_consoleCanvas.activeSelf)
            {
            }
        }

        if (m_cmdLine.isFocused && Input.GetKeyDown(KeyCode.UpArrow))
            m_cmdLine.SetTextWithoutNotify(buffer);



    }

    private void OnEnable()
    {
        m_inputActions.Enable();
    }

    private void OnDisable()
    {
        m_inputActions.Disable();
    }

    private void processCmd(string input) {
        //TODO: use stringbuilder equivalent.
        m_cmdHistory.SetText(m_cmdHistory.text + input +"\n"); 
        m_cmdLine.SetTextWithoutNotify("");

        buffer = input;
        input = input.Trim();
        string cmd = input[..input.IndexOf(' ')];
        string args = input[(input.IndexOf(' ') + 1)..];

        if (cmd.Length == 0)  {
            printError("DID NOT ENTER A COMMAND");
            return;
        }
        
        if (!cmdList.ContainsKey(cmd)) {
            printError("NOT A VALID COMMAND");
            return;
        }

        cmdList[cmd](args);
    }

    private void printError(string errorMsg) {
        m_cmdHistory.SetText(m_cmdHistory.text + "ERROR: " + errorMsg + "\n");
    }

    private void changeBGColour(string hexCode) {
        Color newColour;
        if (ColorUtility.TryParseHtmlString(hexCode, out newColour))
            m_background.GetComponent<Renderer>().material.color = newColour;
        else
            printError("INVALID HEX CODE");
    }

    private void setBallSpeed(string speed) {
        // parse here and set the ball's speed.
        float spd = 10.0f;
        bool success = Single.TryParse(speed, out spd);
        if (success)
            GameObject.FindWithTag("Ball").GetComponent<Ball>().SetSpeed(spd);
        
    }

}
