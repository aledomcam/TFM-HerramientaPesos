using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System.Xml.Linq;

/*
 * Configuración del programa
 */
public class Config {
	public bool válida;

	/*
	 * Variación aplicada al máximo de importancia local para calcular el coeficiente de elasticidad. Debe ser > 0.
	 */
	public int variaciónElasticidad = 1;

	/* 
	 * Crea una nueva instancia de la clase usando una ruta a un fichero de configuración
	 */
	public Config(string ruta) {
		try {
			XElement raíz = XElement.Load(ruta);
			this.válida = true;

			try {
				this.variaciónElasticidad = int.Parse(raíz.Element("variaciónElasticidad").Value);
			} catch (FormatException) {
				Console.Out.WriteLine("Error al leer la configuración: La variación de elasticidad debe ser un entero.");
				this.válida = false;
				return;
			}
		} catch (FileNotFoundException) {
			Console.Out.WriteLine("Error: No se ha encontrado el fichero de configuración. Debería estar ubicado en /datos/config.xml.");
			this.válida = false;
		}

		/*
		 * Comprobación del rango de los valores
		 */
		if (variaciónElasticidad <= 0) {
			Console.Out.WriteLine("Error de configuración: La variación de elasticidad debe ser > 0");
			this.válida = false;
		}
	}
}