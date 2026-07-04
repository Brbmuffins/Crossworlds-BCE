/// <summary>
/// INPCInteractable — interface for any NPC that the player can interact with.
///
/// Implement this on NetworkBehaviour or MonoBehaviour components placed on NPC GameObjects.
/// NPCInteractionManager detects nearby NPCs via collider and calls Interact() on E key.
///
/// PromptText: short verb phrase shown in the bottom-centre interaction prompt.
///   Examples: "Talk to The Hangman", "Browse Merchant", "Open Forge"
/// </summary>
public interface INPCInteractable
{
    /// <summary>Called by NPCInteractionManager when the player presses E near this NPC.</summary>
    void Interact();

    /// <summary>Short prompt text, e.g. "Talk to The Hangman". Max ~40 chars.</summary>
    string PromptText { get; }
}
