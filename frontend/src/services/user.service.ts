import ApiService from "./api.service";
import User from "@/models/User";
import IUserList from "@/models/IUserList";
import config from "@/config";

const UserService = {
  async me(): Promise<User> {
    const idToken = localStorage.getItem("id_token");
    const user: User = idToken ? new User(idToken) : new User();
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
