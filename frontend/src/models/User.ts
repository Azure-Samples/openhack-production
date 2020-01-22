import jsonwebtoken from "jsonwebtoken";

export default class User {
  userName: string = "";
  name: string = "Login / Sign Up";
  loggedIn: boolean = false;
  profileImage: string = "";

  constructor(jwtToken?: string) {
    if (jwtToken) {
      const claims: any = jsonwebtoken.decode(jwtToken);

      this.name = `${claims.given_name} ${claims.family_name}`;
      this.userName = claims.emails[0];
      this.profileImage = "";
      this.loggedIn = true;
    }
  }
}
