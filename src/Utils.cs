using System;
using System.Collections.Generic;

public class Utils {
	/*
	 * Añade espacios a la string "original" hasta llegar a "colInicio" caracteres. A continuación, concatena la string "texto". Si
	 * el resultado excede los "colFin" caracteres, se truncará.
	 */
	public static string concatenarColumna(string original, string texto, int colInicio, int colFin) {
		string res = original;
		
		while (res.Length < colInicio) {
			res += " ";
		}
		res += texto;
		if (res.Length > colFin) {
			res = res.Substring(0, colFin);
		}
		return res;
	}
}