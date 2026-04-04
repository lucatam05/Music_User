namespace Music.User.Shared.Exceptions;

public class ModelNotFoundException(string? message) : Exception(message)
{

}