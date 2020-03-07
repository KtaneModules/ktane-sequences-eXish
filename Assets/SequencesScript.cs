using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;
using System;

public class SequencesScript : MonoBehaviour {

    public KMAudio audio;
    public KMBombInfo bomb;

    public TextMesh[] texts;

    public KMSelectable[] buttons;

    private int[] firstthree = new int[3];
    private string formula;

    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        moduleSolved = false;
        foreach (KMSelectable obj in buttons)
        {
            KMSelectable pressed = obj;
            pressed.OnInteract += delegate () { PressButton(pressed); return false; };
        }
        resetTexts();
        GetComponent<KMBombModule>().OnActivate += OnActivate;
    }

    void Start () {
        generateFormulaAndNums();
    }

    void OnActivate()
    {
        texts[0].text = firstthree[0] + " " + firstthree[1] + " " + firstthree[2];
    }

    void PressButton(KMSelectable pressed)
    {
        if(moduleSolved != true)
        {
            pressed.AddInteractionPunch(0.5f);
            audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, pressed.transform);
            int index = Array.IndexOf(buttons, pressed);
            if (index >= 0 && index <= 9)
            {
                if (texts[1].text.Length < 13)
                    texts[1].text += ""+index;
            }
            else if (index == 10)
            {
                if (texts[1].text.Length < 13)
                    texts[1].text += "n";
            }
            else if (index == 11)
            {
                texts[1].text = "";
            }
            else if (index == 12)
            {
                if (texts[1].text.Length < 13)
                    texts[1].text += "-";
            }
            else if (index == 13)
            {
                if (texts[1].text.Length < 13)
                    texts[1].text += "+";
            }
            else if (index == 14)
            {
                if (texts[1].text.Equals(""))
                    Debug.LogFormat("[Sequences #{0}] Submitted formula: nothing", moduleId);
                else
                    Debug.LogFormat("[Sequences #{0}] Submitted formula: {1}", moduleId, texts[1].text);
                if (formula.Equals(texts[1].text))
                {
                    Debug.LogFormat("[Sequences #{0}] That is correct, module disarmed!", moduleId);
                    audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
                    resetTexts();
                    moduleSolved = true;
                    GetComponent<KMBombModule>().HandlePass();
                }
                else
                {
                    Debug.LogFormat("[Sequences #{0}] That is incorrect, strike!", moduleId);
                    GetComponent<KMBombModule>().HandleStrike();
                }
            }
        }
    }

    void generateFormulaAndNums()
    {
        int type = UnityEngine.Random.Range(0, 2);
        if (type == 0)
        {
            int rando1 = UnityEngine.Random.Range(-99, 100);
            int rando2 = UnityEngine.Random.Range(-99, 100);
            firstthree[0] = (rando1 * 1) + rando2;
            firstthree[1] = (rando1 * 2) + rando2;
            firstthree[2] = (rando1 * 3) + rando2;
            while (rando1 == 0)
            {
                rando1 = UnityEngine.Random.Range(-99, 100);
                rando2 = UnityEngine.Random.Range(-99, 100);
                firstthree[0] = (rando1 * 1) + rando2;
                firstthree[1] = (rando1 * 2) + rando2;
                firstthree[2] = (rando1 * 3) + rando2;
            }
            if (rando2 < 0)
                formula = rando1 + "n" + rando2;
            else if (rando2 > 0)
                formula = rando1 + "n+" + rando2;
            else
                formula = rando1 + "n";
        }
        else if (type == 1)
        {
            int rando1 = UnityEngine.Random.Range(-99, 100);
            int rando2 = UnityEngine.Random.Range(-99, 100);
            firstthree[0] = (rando1 * 1) - rando2;
            firstthree[1] = (rando1 * 2) - rando2;
            firstthree[2] = (rando1 * 3) - rando2;
            while (rando1 == 0)
            {
                rando1 = UnityEngine.Random.Range(-99, 100);
                rando2 = UnityEngine.Random.Range(-99, 100);
                firstthree[0] = (rando1 * 1) - rando2;
                firstthree[1] = (rando1 * 2) - rando2;
                firstthree[2] = (rando1 * 3) - rando2;
            }
            if (rando2 < 0)
                formula = rando1 + "n+" + Math.Abs(rando2);
            else if (rando2 > 0)
                formula = rando1 + "n-" + rando2;
            else
                formula = rando1 + "n";
        }
        if (formula.StartsWith("-1n"))
        {
            formula = formula.Remove(1, 1);
        }
        else if (formula.StartsWith("1n"))
        {
            formula = formula.Remove(0, 1);
        }
        Debug.LogFormat("[Sequences #{0}] The first three numbers in the sequence are: {1} {2} {3}", moduleId, firstthree[0], firstthree[1], firstthree[2]);
        Debug.LogFormat("[Sequences #{0}] The formula for the sequence is: {1}", moduleId, formula);
    }

    void resetTexts()
    {
        texts[0].text = "";
        texts[1].text = "";
    }

    //twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} submit <formula> [Submits the specified formula for the sequence]";
    #pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*submit\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (parameters.Length > 2)
            {
                yield return "sendtochaterror Too many parameters!";
            }
            else if (parameters.Length == 2)
            {
                string[] validchars = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "n", "", "-", "+", "" };
                for (int i = 0; i < parameters[1].Length; i++)
                {
                    if (!validchars.Contains((parameters[1].ElementAt(i)+"").ToLower()))
                    {
                        yield return "sendtochaterror The specified character in the formula '" + parameters[1].ElementAt(i) + "' is invalid!";
                        yield break;
                    }
                }
                if (!texts[1].text.Equals(""))
                {
                    buttons[11].OnInteract();
                    yield return new WaitForSeconds(0.1f);
                }
                for (int i = 0; i < parameters[1].Length; i++)
                {
                    buttons[Array.IndexOf(validchars, (parameters[1].ElementAt(i) + "").ToLower())].OnInteract();
                    yield return new WaitForSeconds(0.1f);
                }
                buttons[14].OnInteract();
            }
            else if (parameters.Length == 1)
            {
                yield return "sendtochaterror Please specify the formula to submit!";
            }
            yield break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        string[] validchars = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "n", "", "-", "+", "" };
        int start = 0;
        if (!texts[1].text.Equals(""))
        {
            if (texts[1].text.Length < formula.Length)
            {
                if (texts[1].text.Equals(formula.Substring(0, texts[1].text.Length)))
                {
                    start = texts[1].text.Length;
                }
            }
            if (start == 0)
            {
                buttons[11].OnInteract();
                yield return new WaitForSeconds(0.1f);
            }
        }
        for (int i = start; i < formula.Length; i++)
        {
            buttons[Array.IndexOf(validchars, formula.ElementAt(i) + "")].OnInteract();
            yield return new WaitForSeconds(0.1f);
        }
        buttons[14].OnInteract();
    }
}
