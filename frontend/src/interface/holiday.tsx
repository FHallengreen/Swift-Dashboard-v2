export interface Holiday {
    date: string;
    localName: string;
    name: string;
    countryCode: string;
    countryName: string | null; // Allow null
    fixed: boolean;
    global: boolean;
    counties: string[] | null;
    launchYear: number | null;
    types: string[] | null;
  }