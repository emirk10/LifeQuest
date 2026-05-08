import axiosClient from '../api/axiosClient';

const API_BASE = axiosClient.defaults.baseURL?.replace('/api', '') || 'http://localhost:5020';

const postService = {
  /**
   * Arkadaşların ve kendi paylaşımlarını getir.
   * @param {number} page - Sayfa numarası (1'den başlar)
   */
  getFeed: async (page = 1, pageSize = 20) => {
    const res = await axiosClient.get(`/posts/feed?page=${page}&pageSize=${pageSize}`);
    return res.data;
  },

  /**
   * Yeni gönderi oluştur. Hem metin hem de resimli paylaşımı destekler.
   * @param {string|null} content - Metin içeriği
   * @param {File|null} imageFile - Resim dosyası
   * @param {Array<{taskId, x, y}>} tags - Resim üzerindeki etiketler
   */
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

  /**
   * Kendi paylaşımını sil.
   * @param {string} postId
   */
  deletePost: async (postId) => {
    const res = await axiosClient.delete(`/posts/${postId}`);
    return res.data;
  },

  /**
   * Gönderiyi beğen.
   * @param {string} postId
   */
  likePost: async (postId) => {
    const res = await axiosClient.post(`/posts/${postId}/like`);
    return res.data;
  },

  /**
   * Gönderi beğenisini kaldır.
   * @param {string} postId
   */
  unlikePost: async (postId) => {
    const res = await axiosClient.delete(`/posts/${postId}/like`);
    return res.data;
  },

  /**
   * Gönderiye yapılan yorumları getir.
   * @param {string} postId
   */
  getComments: async (postId) => {
    const res = await axiosClient.get(`/posts/${postId}/comments`);
    return res.data;
  },

  /**
   * Gönderiye yorum yap.
   * @param {string} postId
   * @param {string} content
   */
  addComment: async (postId, content) => {
    const res = await axiosClient.post(`/posts/${postId}/comments`, { content });
    return res.data;
  },

  /**
   * Resmin tam URL'ini oluştur.
   * @param {string|null} imageUrl - '/uploads/posts/xxx.jpg' formatında
   */
  getImageUrl: (imageUrl) => {
    if (!imageUrl) return null;
    return `${API_BASE}${imageUrl}`;
  },
};

export default postService;
