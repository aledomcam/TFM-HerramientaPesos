using System;
using System.Collections.Generic;

/*
 * Singleton que almacena datos globales a los que necesitan acceder múltiples objetos
 */
public class Global {
	private static Global instancia = null;
	private static readonly object bloqueo = new object();

	// Configuración del programa
	public Config config;
	// RNG
	public Rand rand;

	public Ejecutar ejecutar;
	public ListaCriterios listaCriterios;

	private Global() {
		rand = new Rand();
		config = new Config(Cst.RUTA_CONFIG);
		listaCriterios = new ListaCriterios(this);
		listaCriterios.cargarCriterios(Cst.RUTA_FICHERO_CRITERIOS);
		ejecutar = new Ejecutar(this);
	}

	public static Global getInstancia() {
		lock (bloqueo) {
			if (instancia == null) {
				instancia = new Global();
			}
			return instancia;
		}
	}
}