namespace Application_Livraison_Backend.Dtos
{
    public class CommandeDto
    {
        public required int ClientId { get; set; }
        public required List<int> ProduitsIds { get; set; }
    }
}
