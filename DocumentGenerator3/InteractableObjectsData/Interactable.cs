namespace DocumentGenerator3.InteractableObjectsData
{
    public class Interactable : IInteractable
    {
        public string type { get; set; }
        public string searchText { get; set; }
        public string beforeOrAfterText { get; set; }
        public bool value { get; set; } = true;
        public int nthInstanceOfSearchText { get; set; } = 1;
    }
}
