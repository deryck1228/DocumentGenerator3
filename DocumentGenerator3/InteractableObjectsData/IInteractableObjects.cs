using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentGenerator3.InteractableObjectsData
{
    interface IInteractableObjects
    {
        string template_type { get; set; }
        List<Interactable> interactables { get; set; }
    }
}
