using System;

/*
 * Clase que permite generar distintos tipos de números aleatorios
 */
public class Rand : Random {
	// --- Constantes para el método Monty Python ---
	// Base del rectángulo: sqrt(2*pi)
	private const float B = 2.506628f;
	// Altura
	private const float H = 1/B;
	// Coordenada X de corte entre la parte superior del rectángulo y la función f (normal reflejada)
	private const float A = 1.17741f;
	// Factor de estirado de la zona B de la curva
	private const float S = A / (B - A); // 0.8857913

	/*
	 * Devuelve true con una probabilidad igual al valor indicado (debe estar entre 0 y 1)
	 */
	public bool check(float probabilidad) {
		if (probabilidad >= 0 && probabilidad <= 1) {
			return NextDouble() < probabilidad;
		} else {
			throw new ArgumentOutOfRangeException();
		}
	}

	/*
	 * Devuelve un número aleatorio entre el mínimo (inclusivo) y el máximo (exclusivo) especificados.
	 */
	public double range(double min, double max) {
		return min + NextDouble() * (max - min);
	}

	/*
	 * Genera un valor de la distribución normal con la media y desviación estándares especificadas
	 */
	public float normal(float media = 0, float std = 1) {
		return (float) (media + std * montyPython());
	}

	/*
	 * Lleva a cabo una selección estocástica usando un array de pesos. El resultado es un número al azar entre 0 y
	 * la longitud del array de pesos - 1. La probabilidad de que el elemento i sea elegido será
	 * pesos[i] / (suma de todos los pesos en el array).
	 */
	public int selecciónEstocástica(double[] pesos) {
		if (pesos.Length < 1) {
			throw new ArgumentException("El array de pesos debe tener al menos 1 elemento");
		} else if (pesos.Length == 1) {
			return 0;
		}
		
		int noNulos = 0;
		double pesoTotal = 0;
		for (int i = 0; i < pesos.Length; i++) {
			pesoTotal += pesos[i];
			if (pesos[i] < 0) {
				throw new ArgumentException("El array de pesos no puede contener pesos negativos");
			} else if (pesos[i] > 0) {
				noNulos++;
			}
		}

		if (noNulos == 0) {
			// Devolver uno al azar
			return Next(0, pesos.Length);
		} else {
			double rand = range(0, pesoTotal);
			double pesoAcumulado = 0;
			for (int i = 0; i < pesos.Length; i++) {
				pesoAcumulado += pesos[i];
				if (pesoAcumulado > rand) {
					return i;
				}
			}
			// Probable error de redondeo de decimales
			return pesos.Length - 1;
		}
	}

	/*
	 * Genera un valor de la normal estándar usando el método Monty Python (Marsaglia y Tsang, 1998)
	 */
	private double montyPython() {
		double x = range(-1 * B, B);
		if (Math.Abs(x) < A) {
			return x;
		} else {
			double y = NextDouble();
			if (Math.Log(y) < 0.6931472 - 0.5 * (x * x)) {
				return x;
			} else {
				double dist_x_b;

				if (x > 0) {
					dist_x_b = B - x;
				} else {
					dist_x_b = -1 * (B + x);
				}

				x = dist_x_b * S;

				if (Math.Log(1.8857913 - y) < 0.5718733 - 0.5 * (x * x)) {
					return x; // Estirada
				} else {
					// Método de cola
					while (true) {
						double rand1 = range(-1, 1);
						x = -1 * Math.Log(Math.Abs(rand1)) * H;
						y = -1 * Math.Log(NextDouble());
						if (2 * y >= x * x) {
							return rand1 > 0 ? B + x : -1 * (B + x);
						}
					}
				}
			}
		}
	}
}