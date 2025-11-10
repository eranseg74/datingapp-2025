import { CanDeactivateFn } from '@angular/router';
import { MemberProfile } from '../../features/members/member-profile/member-profile';

// component - the component we are trying to move away from. We change the type in the <> brackets from the default value (unknown) to the component type (MemberProfile) so we will be able to work on it. In this case we want to work on the editForm propery that is inside the MemberProfile component (the component parameter here)
export const preventUnsavedChangesGuard: CanDeactivateFn<MemberProfile> = (
  // The currentRoute, currentState, and nextState are not used but here to know that they exist. They can be removed
  component /* ,
  currentRoute,
  currentState,
  nextState */
) => {
  if (component.editForm?.dirty) {
    // Checking if something in the form was changed
    return confirm('Are you sure you want to continue? All unsaved changes will be lost');
  }
  // If the form was not changed we will return true in order to be able to work in the application
  return true;
};
// This will require to add this gusrd in the 'profile' child in the app.routes.ts as a canDeactivate guard!!!
