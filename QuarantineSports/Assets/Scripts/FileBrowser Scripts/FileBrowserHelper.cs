using System.Collections;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.UI;

namespace FileBrowser_Scripts
{
	[RequireComponent(typeof(InputField))]
	public class FileBrowserHelper : MonoBehaviour
	{
		// Warning: paths returned by FileBrowser dialogs do not contain a trailing '\' character
		// Warning: FileBrowser can only show 1 dialog at a time
		public void StartFileBrowser(bool isVideoFilter)
		{

			FileBrowser.Filter filter = new FileBrowser.Filter("Folders", ".");
			if (isVideoFilter)
			{
				filter = new FileBrowser.Filter("Videos", ".mp4", ".avi");
			}
			
			// Set filters (optional)
			// It is sufficient to set the filters just once (instead of each time before showing the file browser dialog), 
			// if all the dialogs will be using the same filters
			FileBrowser.SetFilters( true, filter);

			// Set default filter that is selected when the dialog is shown (optional)
			// Returns true if the default filter is set successfully
			// In this case, set Images filter as the default filter
			FileBrowser.SetDefaultFilter( isVideoFilter ? ".mp4" : ".");

			// Set excluded file extensions (optional) (by default, .lnk and .tmp extensions are excluded)
			// Note that when you use this function, .lnk and .tmp extensions will no longer be
			// excluded unless you explicitly add them as parameters to the function
			FileBrowser.SetExcludedExtensions( ".lnk", ".tmp", ".zip", ".rar", ".exe" );

			// Add a new quick link to the browser (optional) (returns true if quick link is added successfully)
			// It is sufficient to add a quick link just once
			// Name: Users
			// Path: C:\Users
			// Icon: default (folder icon)
			FileBrowser.AddQuickLink( "Users", "C:\\Users", null );

			// Show a save file dialog 
			// onSuccess event: not registered (which means this dialog is pretty useless)
			// onCancel event: not registered
			// Save file/folder: file, Initial path: "C:\", Title: "Save As", submit button text: "Save"
			// FileBrowser.ShowSaveDialog( null, null, false, "C:\\", "Save As", "Save" );


			// Coroutine example
			StartCoroutine( ShowLoadDialogCoroutine(isVideoFilter) );
		}

		IEnumerator ShowLoadDialogCoroutine(bool isVideoFilter)
		{
			// Show a load file dialog and wait for a response from user
			// Load file/folder: file, Initial path: default (Documents), Title: "Load File", submit button text: "Load"
			if (! isVideoFilter)
			{
				// Show a select folder dialog 
				// onSuccess event: print the selected folder's path
				// onCancel event: print "Canceled"
				// Load file/folder: folder, Initial path: default (Documents), Title: "Select Folder", submit button text: "Select"
				yield return FileBrowser.WaitForLoadDialog(true, null, "Select Folder", "Select" );
			}
			else
			{
				yield return FileBrowser.WaitForLoadDialog( false, null, "Select Video", "Select" );
			}

			if( FileBrowser.Success )
			{
				InputField field = gameObject.GetComponent<InputField>();
				field.text = FileBrowser.Result + (isVideoFilter ? "" : "\\");
			}
		}
	}
}
