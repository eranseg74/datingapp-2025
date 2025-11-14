export type Member = {
  id: string;
  dateOfBirth: string;
  imageUrl?: string;
  displayName: string;
  created: string;
  lastActive: string;
  gender: string;
  description?: string;
  city: string;
  country: string;
};

export type Photo = {
  id: number;
  url: string;
  publicId?: string;
  memberId: string;
};

export type EditableMember = {
  displayName: string;
  description?: string;
  city: string;
  country: string;
};

// Exporting a class so it will allow us to initialize some properties as well as define it as a type
export class MemberParams {
  gender?: string;
  minAge = 18;
  maxAge = 100;
  pageNumber = 1;
  pageSize = 10;
  orderBy = 'lastActive';
}
