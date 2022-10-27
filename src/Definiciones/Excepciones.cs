/*
 * Excepción usada para representar una situación imposible o errónea
 */
public class IllegalStateException : System.Exception {
	public IllegalStateException() : base() { }
	public IllegalStateException(string message) : base(message) { }
	public IllegalStateException(string message, System.Exception inner) : base(message, inner) { }
	protected IllegalStateException(System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}

/*
 * Excepción que tiene lugar cuando se intenta realizar una operación no permitida o imposible dado el estado actual
 */
public class IllegalOperationException : System.Exception {
	public IllegalOperationException() : base() { }
	public IllegalOperationException(string message) : base(message) { }
	public IllegalOperationException(string message, System.Exception inner) : base(message, inner) { }
	protected IllegalOperationException(System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}