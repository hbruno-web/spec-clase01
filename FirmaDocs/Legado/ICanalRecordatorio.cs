namespace FirmaDocs.Legado;

// Canal por el que salen los recordatorios (en producción, correo).
public interface ICanalRecordatorio
{
    void Enviar(string email, string mensaje);
}
