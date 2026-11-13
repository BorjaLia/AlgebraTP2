using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Linq;

public class SetMethodsTester : MonoBehaviour
{
    private class ModuloComparer : IEqualityComparer<int>
    {
        public bool Equals(int x, int y) => (x % 5) == (y % 5);
        public int GetHashCode(int obj) => (obj % 5).GetHashCode();
    }

    [SerializeField] private InputActionReference runTestAction;


    void Start()
    {
        RunAllTests();

        if (runTestAction != null)
        {
            runTestAction.action.performed += RunAllTestsCallback;
            runTestAction.action.Enable();
        }
    }

    private void RunAllTestsCallback(InputAction.CallbackContext context)
    {
        RunAllTests();
    }

    void RunAllTests()
    {
        List<int> setA = new List<int>();
        List<int> setB = new List<int>();

        for (int i = 0; i < UnityEngine.Random.Range(5, 15); i++)
        {
            setA.Add(UnityEngine.Random.Range(5, 20));
            setB.Add(UnityEngine.Random.Range(10, 25));
        }

        Debug.Log($"--- INITIALIZED SETS ---");
        Debug.Log($"Set A: [{string.Join(", ", setA)}]");
        Debug.Log($"Set B: [{string.Join(", ", setB)}]");
        Debug.Log($"------------------------");

        ModuloComparer customComparer = new ModuloComparer();

        Debug.Log($"All (> 0): {SetMethods.All(setA, x => x > 0)}");
        Debug.Log($"Any (> 50): {SetMethods.Any(setA, x => x > 50)}");

        int searchItem = setA[0];
        Debug.Log($"Contains {searchItem} (Default): {SetMethods.Contains(setA, searchItem)}");
        Debug.Log($"Contains {searchItem + 5} (Custom Modulo 5): {SetMethods.Contains(setA, searchItem + 5, customComparer)}");

        Debug.Log($"Distinct (Default): [{string.Join(", ", SetMethods.Distinct(setA))}]");
        Debug.Log($"Distinct (Custom Modulo 5): [{string.Join(", ", SetMethods.Distinct(setA, customComparer))}]");

        try
        {
            Debug.Log($"ElementAt index 3: {SetMethods.ElementAt(setA, 3)}");
            Debug.Log($"ElementAt index 10 (Should Fail): {SetMethods.ElementAt(setA, 10)}");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"ElementAt Failed (Expected): {e.Message}");
        }

        Debug.Log($"Except Set B (Default): [{string.Join(", ", SetMethods.Except(setA, setB))}]");
        Debug.Log($"Except Set B (Custom Modulo 5): [{string.Join(", ", SetMethods.Except(setA, setB, customComparer))}]");

        try
        {
            Debug.Log($"First even number: {SetMethods.First(setA, x => x % 2 == 0)}");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"First Failed (Expected if no even numbers): {e.Message}");
        }

        try
        {
            Debug.Log($"Last even number: {SetMethods.Last(setA, x => x % 2 == 0)}");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Last Failed (Expected if no even numbers): {e.Message}");
        }

        Debug.Log($"Intersect Set B (Default): [{string.Join(", ", SetMethods.Intersect(setA, setB))}]");
        Debug.Log($"Intersect Set B (Custom Modulo 5): [{string.Join(", ", SetMethods.Intersect(setA, setB, customComparer))}]");

        Debug.Log($"Count (> 10): {SetMethods.Count(setA, x => x > 10)}");

        List<int> copyA = new List<int>(setA);
        Debug.Log($"SequenceEqual to Copy A: {SetMethods.SequenceEqual(setA, copyA, null)}");
        Debug.Log($"SequenceEqual to Set B: {SetMethods.SequenceEqual(setA, setB, null)}");

        try
        {
            Debug.Log($"Single (> 10): {SetMethods.Single(setA, x => x > 10)}");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Single Failed (Expected if 0 or >1 matches found): {e.Message}");
        }

        Debug.Log($"SkipWhile (< 15): [{string.Join(", ", SetMethods.SkipWhile(setA, x => x < 15))}]");

        Debug.Log($"Union Set B (Default): [{string.Join(", ", SetMethods.Union(setA, setB))}]");
        Debug.Log($"Union Set B (Custom Modulo 5): [{string.Join(", ", SetMethods.Union(setA, setB, customComparer))}]");

        Debug.Log($"Where (> 10): [{string.Join(", ", SetMethods.Where(setA, x => x > 10))}]");
    }
}