namespace FirmaDocs;

   /// <summary>Estado de un documento dentro de su circuito de firma.</summary>
   public enum EstadoDocumento
   {
       /// <summary>El autor todavía no lo envió a firma.</summary>
       Borrador,

       /// <summary>Enviado: espera la aprobación del firmante en turno.</summary>
       EnCircuito,

       /// <summary>Todos los firmantes aprobaron la versión vigente.</summary>
       Aprobado
   }