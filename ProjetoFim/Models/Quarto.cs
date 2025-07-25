using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace ProjetoFim.Models
{
    public class Quarto
    {
        public Quarto()
        {
            this.Comodidades = new HashSet<Comodidade>();
            this.Imagens = new HashSet<Imagem>();
        }

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        public string Nome { get; set; }

        public string Descricao { get; set; }

        [Required(ErrorMessage = "A localização é obrigatória.")]
        public string Localizacao { get; set; }

        [Required(ErrorMessage = "O preço é obrigatório.")]
        [DisplayName("Preço por Noite")]
        public decimal PrecoPorNoite { get; set; }

        [Required(ErrorMessage = "O número de hóspedes é obrigatório.")]
        [DisplayName("Nº de Hóspedes")]
        public int NumeroHospedes { get; set; }

        // --- PROPRIEDADES DE INSTALAÇÕES ---
        [DisplayName("Camas de Casal")]
        public int NumeroCamasCasal { get; set; }

        [DisplayName("Camas de Solteiro")]
        public int NumeroCamasSolteiro { get; set; }

        [DisplayName("Casas de Banho")]
        public int NumeroCasasDeBanho { get; set; }

        [DisplayName("Estacionamento")]
        public bool TemEstacionamento { get; set; }

        // --- PROPRIEDADES DE NAVEGAÇÃO ---

        // Um Quarto pode ter uma coleção de Imagens
        public virtual ICollection<Imagem> Imagens { get; set; }

        // Um Quarto pode ter uma coleção de Comodidades
        public virtual ICollection<Comodidade> Comodidades { get; set; }
    }
}