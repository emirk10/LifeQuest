import axiosClient from '../api/axiosClient';

const API_BASE = axiosClient.defaults.baseURL?.replace('/api', '') || 'http://localhost:5020';

const postService = {
  



  getFeed: async (page = 1, pageSize = 20) => {
    const res = await axiosClient.get(`/posts/feed?page=${page}&pageSize=${pageSize}`);
    return res.data;
  },

  





  createPost: async (content, imageFile, tags = []) => {
    const formData = new FormData();
    if (content) formData.append('Content', content);
    if (imageFile) formData.append('Image', imageFile);
    if (tags.length > 0) formData.append('TagsJson', JSON.stringify(tags));

    const res = await axiosClient.post('/posts', formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    });
    return res.data;
  },

  



  deletePost: async (postId) => {
    const res = await axiosClient.delete(`/posts/${postId}`);
    return res.data;
  },

  



  likePost: async (postId) => {
    const res = await axiosClient.post(`/posts/${postId}/like`);
    return res.data;
  },

  



  unlikePost: async (postId) => {
    const res = await axiosClient.delete(`/posts/${postId}/like`);
    return res.data;
  },

  



  getComments: async (postId) => {
    const res = await axiosClient.get(`/posts/${postId}/comments`);
    return res.data;
  },

  




  addComment: async (postId, content) => {
    const res = await axiosClient.post(`/posts/${postId}/comments`, { content });
    return res.data;
  },

  



  getImageUrl: (imageUrl) => {
    if (!imageUrl) return null;
    return `${API_BASE}${imageUrl}`;
  },
};

export default postService;
