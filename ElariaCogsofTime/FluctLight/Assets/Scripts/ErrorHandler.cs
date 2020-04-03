
/*
 * ----- Error Handler Class -----
 *
 * Parent Class for all combat entities, including player characters 
 * and enemy characters.
 *
 */

public class ErrorHandler
{
    // Log and throw exception for loading resources
    public void ResourceLoadError(string pMessage = "")
    {
        throw new System.Exception("Could not load resources(s): " + pMessage);
    }

    //
    public void MovePreviewEmptyError (string pMessage = "")
    {
        throw new System.Exception("The grid vertex list for movement preview is either NULL or EMPTY: " + pMessage);
    }
}
