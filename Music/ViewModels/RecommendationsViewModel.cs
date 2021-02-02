using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Music.ViewModels
{
    public class RecommendationsViewModel
    {
        public string UserId { get; set; }

        // ova lista ce uvijek biti oblika ("artist", "category", "published")
        // za svaku imamo jedan option u select 
        public List<string> SearchAttributes { get; set; }

        // atribut koji je od prethodna 3 oznacen (po njemu znamo kako pretrazivati bazu)
        public string SelectedAttribute { get; set; }

        // sve moguce vrijednosti atributa 
        // (primjerice ako u FAVORITES imamo 5 izvodaca ova lista ce imati 5 vrijednosti)
       // za svaku imamo jedan checkbox (osvjezava se na onChange selecta)
        public List<string> SearchValues { get; set; } 

        // lista koja sadrzi sve oznacene vrijednosti iz prethodne liste
        // te vrijednosti cemo slati u upitu za bazu podataka
        public List<string> SelectedValues { get; set; }

        // dictionary nam ipak ne treba tu jer ce se vracati nova stranica 
        // sa dictionarijem pjesama (koje ce biti podijeljene u vise tablica)
        // (svaki kljuc u Dictionary) predstavlja jednu tablicu

        public RecommendationsViewModel(string uID, List<string> attributes)
        {
            UserId = uID;
            SearchAttributes = attributes;
        }
    }
}
