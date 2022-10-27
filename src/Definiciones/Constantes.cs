// Constantes misceláneas que no encajan en una sola clase
public static class Cst {
	// --- Rutas de ficheros ---
	public const string RUTA_DATOS = "datos/";
	public const string RUTA_CONFIG = RUTA_DATOS + "config.xml";
	public const string RUTA_FICHERO_CRITERIOS = RUTA_DATOS + "criterios.xml";
	public const string RUTA_LBWA = RUTA_DATOS + "LBWA.xml";
	public const string RUTA_BWM = RUTA_DATOS + "BWM.xml";
	public const string RUTA_SISMOS = RUTA_DATOS + "Sismos.xml";
	public const string RUTA_ROC = RUTA_DATOS + "ROC.xml";

	// --- LBWA ---
	// Número máximo de niveles que se pueden usar en LBWA
	public const int MAX_NIVELES_LBWA = 20;

	// --- BWM ---
	/*
	 * Almacenan la posición (columna) en la que colocar los diferentes campos de las instancias de ConsistenciaBWM
	 * al mostrarlas por pantalla
	 */
	public const int POS_ID_CRITERIO = 0;
	public const int POS_MEJOR_A_ESTE = POS_ID_CRITERIO + 20;
	public const int POS_ESTE_A_PEOR = POS_MEJOR_A_ESTE + 4;
	public const int POS_CONSISTENCIA_ENTRADA = POS_ESTE_A_PEOR + 4;
	public const int POS_CONSISTENCIA_ORDINAL = POS_CONSISTENCIA_ENTRADA + 12;

	// --- Misc ---
	// Nombre usado al imprimir la familia de un nodo que no tiene padre
	public const string NOMBRE_FAMILIA_RAÍZ = "(raíz)";
}