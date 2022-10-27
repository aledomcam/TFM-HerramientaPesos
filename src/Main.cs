using System;
using System.Collections.Generic;

public class MainClass {
	private const string STR_SALIR = "s";
	private const string STR_LBWA = "lbwa";
	private const string STR_BWM = "bwm";
	private const string STR_SISMOS = "sismos";
	private const string STR_ROC = "roc";
	private const string STR_IMPRIMIR_PESOS_GLOBALES = "imprimirG";
	private const string STR_IMPRIMIR_PESOS_LOCALES = "imprimirL";
	private const string STR_GUARDAR_PESOS = "guardar";

	static void Main(string[] args) {
		Global G = Global.getInstancia();
		if (G.config.válida) {
			menúPrincipal();
		} else {
			Console.Error.WriteLine("La configuración del programa no se ha podido cargar o es inválida. Abortando.");
		}
	}

	private static void menúPrincipal() {
		printCabecera();
		bool salir;
		do {
			printOpciones();
			salir = solicitarOpción();
		} while (!salir);
	}

	/*
	 * Muestra el texto asociado al menú principal del programa
	 */
	private static void printCabecera() {
		Console.Out.WriteLine("-----------------------------------");
		Console.Out.WriteLine("-----------------------------------");
		Console.Out.WriteLine("  Herramienta de cálculo de pesos");
		Console.Out.WriteLine("        MUIA - TFM 2021/22");
		Console.Out.WriteLine("     Alejandro Domínguez Campos");
		Console.Out.WriteLine("-----------------------------------");
		Console.Out.WriteLine("-----------------------------------");
	}

	private static void printOpciones() {
		Console.Out.WriteLine("--- Selecciona una opción ---");
		Console.Out.WriteLine("    " + STR_SALIR + ": Salir");
		Console.Out.WriteLine("    " + STR_LBWA + ": Ejecutar el método LBWA para obtener los valores de los pesos. Requiere que existan los datos LBWA en " + Cst.RUTA_LBWA + ".");
		Console.Out.WriteLine("    " + STR_BWM + ": Ejecutar el método BWM para determinar el nivel de consistencia (ordinal y cardinal) de las comparaciones especificadas. Requiere que existan los datos BWM en " + Cst.RUTA_BWM + ".");
		Console.Out.WriteLine("    " + STR_SISMOS + ": Ejecutar el método de Sismos revisado para obtener los valores de los pesos. Requiere que existan los datos Sismos en " + Cst.RUTA_SISMOS + ".");
		Console.Out.WriteLine("    " + STR_ROC + ": Ejecutar el método de pesos ROC para obtener los valores de los pesos. Requiere que existan los datos ROC en " + Cst.RUTA_ROC + ".");
		Console.Out.WriteLine("    " + STR_IMPRIMIR_PESOS_GLOBALES + ": Muestra por pantalla el peso global calculado de cada criterio para cada método.");
		Console.Out.WriteLine("    " + STR_IMPRIMIR_PESOS_LOCALES + ": Muestra por pantalla el peso local calculado de cada criterio para cada método.");
		Console.Out.WriteLine("    " + STR_GUARDAR_PESOS + " <NombreFicheroSalida>: Genera un fichero XML de salida con los pesos calculados.");
	}

	/*
	 * Return: True si la ejecución del programa debe terminar
	 */
	private static bool solicitarOpción() {
		Ejecutar ejecutar = Global.getInstancia().ejecutar;
		string input = Console.ReadLine();
		string[] inputDividido = input.Split();

		if (inputDividido[0] == STR_SALIR) {
			// Nada que hacer
			return true;
		} else if (inputDividido[0] == STR_LBWA) {
			ejecutar.ejecutarLBWA();
			return false;
		} else if (inputDividido[0] == STR_BWM) {
			ejecutar.consistenciaBWM();
			return false;
		} else if (inputDividido[0] == STR_SISMOS) {
			ejecutar.ejecutarSismos();
			return false;
		} else if (inputDividido[0] == STR_ROC) {
			ejecutar.ejecutarRoc();
			return false;
		} else if (inputDividido[0] == STR_IMPRIMIR_PESOS_GLOBALES) {
			ejecutar.printPesosGlobales();
			return false;
		} else if (inputDividido[0] == STR_IMPRIMIR_PESOS_LOCALES) {
			ejecutar.printPesosLocales();
			return false;
		} else if (inputDividido[0] == STR_GUARDAR_PESOS) {
			if (inputDividido.Length < 2) {
				Console.Out.WriteLine("Error: Debe especificarse la ruta al fichero de salida");
			} else {
				ejecutar.exportarPesos(inputDividido[1]);
			}
			return false;
		} else {
			Console.Out.WriteLine("\"" + inputDividido[0] + "\" no es una opción válida");
			return false;
		}
	}
}