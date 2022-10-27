# TFM-HerramientaPesos
Este repositorio contiene una herramienta auxiliar empleada durante la realización de mi Trabajo de Fin de Máster, que se puede consultar [en el Archivo Digital de la UPM](https://oa.upm.es/71403/).

La herramienta está desarrollada en C# y se puede utilizar para obtener pesos de criterios en base a la información proporcionada por un decisor, permitiendo emplear diferentes métodos de obtención de preferencias. Concretamente, la herramienta soporta los siguientes métodos:

- Best Worst Method ([Rezaei, 2015](https://doi.org/10.1016/j.omega.2014.11.009))
	- Para este método, la herramienta solo puede calcular valores de consistencia, no obtener los pesos finales de cada criterio. Esto es debido a que el cálculo de los pesos finales por este método no es trivial.
- Método de Sismos revisado ([Figueira y Roy, 2002](https://doi.org/10.1016/S0377-2217(01)00370-8))
- Método ROC ([Edwards y Barron, 1994](https://doi.org/10.1006/obhd.1994.1087))
- Level Based Weight Assessment ([Žižovic y Pamucar, 2019](https://www.dmame.rabek.org/index.php/dmame/article/view/48))

La carpeta `datos` contiene un ejemplo con los ficheros que se pueden usar como entrada para los diferentes métodos que puede ejecutar el programa.