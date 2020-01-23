import ApiService from "./api.service";
import User from "@/models/User";
import IUserList from "@/models/IUserList";
import config from "@/config";
import axios from "axios";

const UserService = {
  async me(): Promise<User> {
    const idToken = localStorage.getItem("id_token");
    const user: User = idToken ? new User(idToken) : new User();

    if (user.loggedIn) {
      const authorizationHeaderValue = `Bearer ${localStorage.getItem("access_token")}`;
      axios.defaults.headers.common["Authorization"] = authorizationHeaderValue;
    } else {
      axios.defaults.headers.common["Authorization"] = "";
    }

    return Promise.resolve(user);
  },

  async lists(userName: string): Promise<Array<IUserList>> {
    const response = await ApiService.get(
      `${config.BACKEND}/api/links/user/${userName}`
    );
    return response ? <Array<IUserList>>response.data : [];
  }
};

export default UserService;
