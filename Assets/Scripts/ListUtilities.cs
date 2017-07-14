using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ListUtilities
{

    public static string ToStringNew<T>(this List<T> list) {
        string s = "";
        for(int i = 0; i < list.Count; i++) {
            s += list[i].ToString() + "\n";
        }
        return s;
    }
}
