using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentGenerator3.InteractableObjectsData
{
    public class InteractableObjects : IInteractableObjects
    {
        public string template_type { get; set; }
        public List<Interactable> interactables { get; set; }
    }
}
