import React, { useState, useEffect, useCallback } from 'react';
import MainLayout from '../layouts/MainLayout';
import postService from '../services/postService';
import socialService from '../services/socialService';
import taskService from '../services/taskService';
import { toast, ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import { useAuth } from '../context/authContext';


const PostCard = ({ post, currentUserId, onDelete }) => {
  const [liked, setLiked] = useState(post.isLikedByMe);
  const [likesCount, setLikesCount] = useState(post.likeCount || 0);
  const [showComments, setShowComments] = useState(false);
  const [comments, setComments] = useState([]);
  const [commentCount, setCommentCount] = useState(post.commentCount || 0);
  const [newComment, setNewComment] = useState('');
  const [loadingComments, setLoadingComments] = useState(false);
  const [submittingComment, setSubmittingComment] = useState(false);

  const imageUrl = postService.getImageUrl(post.imageUrl);
  const isOwner = post.author.id === currentUserId;

  const avatarColors = ['bg-indigo-600', 'bg-violet-600', 'bg-emerald-600', 'bg-rose-600', 'bg-amber-600'];
  const avatarColor = avatarColors[post.author.userName?.charCodeAt(0) % avatarColors.length] || 'bg-indigo-600';

  const timeAgo = (date) => {
    const diff = (Date.now() - new Date(date).getTime()) / 1000;
    if (diff < 60) return 'Az önce';
    if (diff < 3600) return `${Math.floor(diff / 60)} dk önce`;
    if (diff < 86400) return `${Math.floor(diff / 3600)} sa önce`;
    return `${Math.floor(diff / 86400)} gün önce`;
  };

  const handleLike = async () => {
    try {
      if (liked) {
        setLiked(false);
        setLikesCount(prev => Math.max(0, prev - 1));
        await postService.unlikePost(post.id);
      } else {
        setLiked(true);
        setLikesCount(prev => prev + 1);
        await postService.likePost(post.id);
      }
    } catch (err) {
      setLiked(!liked);
      setLikesCount(prev => (liked ? prev + 1 : prev - 1));
      toast.error('Beğeni işlemi başarısız oldu.');
    }
  };

  const toggleComments = async () => {
    if (showComments) {
      setShowComments(false);
      return;
    }
    setShowComments(true);
    setLoadingComments(true);
    try {
      const data = await postService.getComments(post.id);
      setComments(data.comments || []);
    } catch (err) {
      toast.error('Yorumlar yüklenemedi.');
    } finally {
      setLoadingComments(false);
    }
  };

  const handleAddComment = async (e) => {
    e.preventDefault();
    if (!newComment.trim() || submittingComment) return;
    setSubmittingComment(true);
    try {
      await postService.addComment(post.id, newComment);
      setNewComment('');
      setCommentCount(prev => prev + 1);
      const data = await postService.getComments(post.id);
      setComments(data.comments || []);
    } catch (err) {
      toast.error('Yorum yapılamadı.');
    } finally {
      setSubmittingComment(false);
    }
  };

  return (
    <div className="bg-slate-900/60 border border-slate-800 rounded-3xl overflow-hidden hover:border-indigo-500/20 transition-all duration-300 group">
      {}
      <div className="flex items-center justify-between p-5 pb-3">
        <div className="flex items-center gap-3">
          <div className={`w-10 h-10 ${avatarColor} rounded-2xl flex items-center justify-center font-black text-white`}>
            {post.author.fullName?.[0]?.toUpperCase() || '?'}
          </div>
          <div>
            <p className="font-bold text-white text-sm">{post.author.fullName}</p>
            <p className="text-slate-500 text-xs">@{post.author.userName} · Lv.{post.author.level}</p>
          </div>
        </div>
        <div className="flex items-center gap-2">
          <span className="text-slate-600 text-xs">{timeAgo(post.createdAt)}</span>
          {isOwner && (
            <button
              onClick={() => onDelete(post.id)}
              className="text-slate-600 hover:text-rose-500 transition-colors p-1 rounded-lg hover:bg-rose-500/10"
              title="Gönderiyi sil"
            >
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
              </svg>
            </button>
          )}
        </div>
      </div>

      {}
      {post.content && (
        <p className="px-5 pb-3 text-slate-300 text-sm leading-relaxed">{post.content}</p>
      )}

      {}
      {imageUrl && (
        <div className="relative mx-4 mb-4 rounded-2xl overflow-hidden bg-slate-800">
          <img
            src={imageUrl}
            alt="Gönderi görseli"
            className="w-full h-auto max-h-[500px] object-cover"
            style={{ display: 'block' }}
          />
        </div>
      )}

      {}
      {post.tags?.length > 0 && (
        <div className="px-5 pb-4 flex flex-wrap gap-2">
          {post.tags.map((tag) => (
            <div key={tag.id} className="flex items-center justify-between w-full bg-indigo-500/10 border border-indigo-500/20 px-3 py-2 rounded-xl">
              <div className="flex items-center gap-2">
                <span className="text-lg">🏆</span>
                <div>
                  <p className="text-[10px] text-indigo-400 font-black uppercase tracking-wider">Tamamlanan Görev</p>
                  <p className="text-white text-xs font-bold">{tag.taskTitle}</p>
                </div>
              </div>
              {!isOwner && (
                <button
                  onClick={async () => {
                    try {
                      const userTaskId = tag.userTaskId || tag.id;
                      const res = await socialService.sendKudos(userTaskId);
                      if (res.success) {
                        toast.success(res.message || 'Kudos gönderildi! 🌟');
                      } else {
                        toast.info(res.message || 'Kudos zaten gönderilmiş.');
                      }
                    } catch (err) {
                      toast.error(err.response?.data?.message || 'Kudos gönderilemedi.');
                    }
                  }}
                  className="bg-indigo-600/20 hover:bg-indigo-600 text-indigo-400 hover:text-white px-3 py-1.5 rounded-lg text-[10px] font-black uppercase tracking-widest transition-all"
                >
                  🌟 Kudos Ver
                </button>
              )}
            </div>
          ))}
        </div>
      )}

      {}
      <div className="px-5 py-4 border-t border-slate-800/50 flex items-center gap-6">
        <button 
          onClick={handleLike}
          className={`flex items-center gap-2 transition-all duration-300 ${liked ? 'text-rose-500' : 'text-slate-400 hover:text-rose-400'}`}
        >
          <svg className={`w-6 h-6 ${liked ? 'fill-current' : 'fill-none'}`} stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z" />
          </svg>
          <span className="text-xs font-bold">{likesCount}</span>
        </button>

        <button 
          onClick={toggleComments}
          className={`flex items-center gap-2 transition-all duration-300 ${showComments ? 'text-indigo-400' : 'text-slate-400 hover:text-indigo-400'}`}
        >
          <svg className="w-6 h-6 fill-none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
          </svg>
          <span className="text-xs font-bold">{commentCount}</span>
        </button>
      </div>

      {}
      {showComments && (
        <div className="px-5 pb-5 pt-2 border-t border-slate-800/30 animate-in slide-in-from-top-2 duration-300">
          {}
          <form onSubmit={handleAddComment} className="flex gap-3 mb-4">
            <input
              type="text"
              value={newComment}
              onChange={(e) => setNewComment(e.target.value)}
              placeholder="Bir yorum yap..."
              className="flex-1 bg-slate-800/50 border border-slate-700 rounded-xl px-4 py-2 text-xs text-white placeholder-slate-600 focus:outline-none focus:border-indigo-500/50"
            />
            <button 
              type="submit"
              disabled={!newComment.trim() || submittingComment}
              className="bg-indigo-600 hover:bg-indigo-500 disabled:opacity-30 text-white text-[10px] font-black uppercase tracking-wider px-4 py-2 rounded-xl transition-all"
            >
              Gönder
            </button>
          </form>

          {}
          <div className="space-y-4 max-h-60 overflow-y-auto pr-2 custom-scrollbar">
            {loadingComments ? (
              <div className="py-4 text-center">
                <div className="w-4 h-4 border-2 border-indigo-500 border-t-transparent rounded-full animate-spin mx-auto" />
              </div>
            ) : comments.length === 0 ? (
              <p className="text-slate-600 text-[10px] italic text-center py-4">Henüz yorum yok. İlk yorumu sen yap! 💬</p>
            ) : (
              comments.map((comment) => (
                <div key={comment.id} className="flex gap-3 animate-in fade-in duration-500">
                  <div className={`w-7 h-7 rounded-lg flex-shrink-0 flex items-center justify-center font-bold text-[10px] text-white bg-slate-700`}>
                    {comment.author.fullName?.[0]?.toUpperCase()}
                  </div>
                  <div className="flex-1">
                    <div className="flex items-center justify-between gap-2 mb-0.5">
                      <span className="text-xs font-bold text-slate-300">{comment.author.fullName}</span>
                      <span className="text-[10px] text-slate-600">{timeAgo(comment.createdAt)}</span>
                    </div>
                    <p className="text-xs text-slate-400 leading-relaxed">{comment.content}</p>
                  </div>
                </div>
              ))
            )}
          </div>
        </div>
      )}
    </div>
  );
};


const ITEM_TYPES = [
  { id: 'task',      label: 'Görevler',    icon: '⚔️',  color: 'indigo' },
  { id: 'goal',      label: 'Hedefler',    icon: '🎯',  color: 'teal'   },
  { id: 'challenge', label: 'Mücadeleler', icon: '🏆',  color: 'rose'   },
];

const COLOR_MAP = {
  indigo: { bg: 'bg-indigo-500/10', border: 'border-indigo-500/30', text: 'text-indigo-400', active: 'bg-indigo-500/20 border-indigo-500/50' },
  teal:   { bg: 'bg-teal-500/10',   border: 'border-teal-500/30',   text: 'text-teal-400',   active: 'bg-teal-500/20 border-teal-500/50'   },
  rose:   { bg: 'bg-rose-500/10',   border: 'border-rose-500/30',   text: 'text-rose-400',   active: 'bg-rose-500/20 border-rose-500/50'   },
};


const isWithin7Days = (dateStr) => {
  if (!dateStr) return false;
  return (Date.now() - new Date(dateStr).getTime()) <= 7 * 24 * 60 * 60 * 1000;
};


const dayLabel = (dateStr) => {
  if (!dateStr) return 'Tarih yok';
  const diff = Math.floor((Date.now() - new Date(dateStr).getTime()) / 86400000);
  if (diff === 0) return 'Bugün';
  if (diff === 1) return 'Dün';
  return `${diff} gün önce`;
};


const groupByDay = (items, getDate) => {
  const groups = {};
  items.forEach(item => {
    const label = dayLabel(getDate(item));
    if (!groups[label]) groups[label] = [];
    groups[label].push(item);
  });
  
  return Object.entries(groups).sort((a, b) => {
    const order = (l) => l === 'Bugün' ? 0 : l === 'Dün' ? 1 : parseInt(l);
    return order(a[0]) - order(b[0]);
  });
};

const CreatePostForm = ({ onPostCreated, userTasks, completedGoals, completedChallenges }) => {
  const [content, setContent] = useState('');
  const [imageFile, setImageFile] = useState(null);
  const [imagePreview, setImagePreview] = useState(null);
  const [tags, setTags] = useState([]);
  const [submitting, setSubmitting] = useState(false);
  const [showPicker, setShowPicker] = useState(false);
  const [pickerType, setPickerType] = useState('task');

  
  const filteredTasks = userTasks.filter(t => {
    const isDone = t.status === 'Completed' || t.Status === 'Completed'
      || t.isCompleted === true || t.IsCompleted === true;
    if (!isDone) return false;
    const completedDate = t.completedAt || t.CompletedAt;
    
    if (!completedDate) return true;
    return isWithin7Days(completedDate);
  });

  const filteredGoals = completedGoals.filter(g => isWithin7Days(g.completedAt));
  const filteredChallenges = completedChallenges.filter(c =>
    c.status === 'Completed' && isWithin7Days(c.endDate)
  );

  const handleSelectItem = (item, type) => {
    const id = item.appTaskId || item.AppTaskId || item.id || item.Id;
    if (tags.some(t => t.taskId === id)) { setShowPicker(false); return; }
    let label = '';
    if (type === 'task')      label = item.title || item.Title;
    if (type === 'goal')      label = `🎯 ${item.title}`;
    if (type === 'challenge') label = `🏆 ${item.title}`;
    setTags(prev => [...prev, { taskId: id, taskTitle: label, type, x: 0, y: 0 }]);
    setShowPicker(false);
  };

  const removeTag = (idx) => setTags(prev => prev.filter((_, i) => i !== idx));

  const handleImageChange = (e) => {
    const file = e.target.files?.[0];
    if (!file) return;
    setImageFile(file);
    setImagePreview(URL.createObjectURL(file));
  };

  const handleSubmit = async () => {
    if (tags.length === 0) {
      toast.warning('Lütfen paylaşmak istediğin bir başarı seç! 🎯');
      return;
    }
    if (!content.trim() && !imageFile) {
      toast.error('Lütfen bir metin veya görsel ekle.');
      return;
    }
    setSubmitting(true);
    try {
      await postService.createPost(
        content || null,
        imageFile || null,
        tags.map(({ taskId, x, y }) => ({ taskId, x, y }))
      );
      toast.success('Başarın paylaşıldı! 🎉');
      setContent(''); setImageFile(null); setImagePreview(null); setTags([]);
      onPostCreated();
    } catch (err) {
      const errorMsg = err.response?.data?.error_detail || err.response?.data?.message || 'Paylaşım yapılamadı.';
      toast.error(errorMsg);
    } finally {
      setSubmitting(false);
    }
  };

  
  const pickerItems  = pickerType === 'task' ? filteredTasks : pickerType === 'goal' ? filteredGoals : filteredChallenges;
  const pickerGetDate = pickerType === 'task'
    ? (i) => i.completedAt || i.CompletedAt
    : pickerType === 'goal'
    ? (i) => i.completedAt
    : (i) => i.endDate;
  const grouped = groupByDay(pickerItems, pickerGetDate);
  const activeType = ITEM_TYPES.find(t => t.id === pickerType);
  const cm = COLOR_MAP[activeType.color];

  return (
    <div className="bg-slate-900/60 border border-slate-800 rounded-3xl p-6 mb-8">
      <h3 className="text-white font-black text-sm uppercase tracking-widest mb-4 flex items-center gap-2">
        <span className="w-2 h-2 bg-indigo-500 rounded-full animate-pulse" />
        Başarını Paylaş
      </h3>

      {}
      <div className="mb-4">
        <label className="text-[10px] text-slate-500 font-black uppercase tracking-widest mb-2 block">Başarı Seç (Zorunlu)</label>
        <div className="flex flex-wrap gap-2 mb-2">
          {tags.map((tag, idx) => {
            const tc = tag.type === 'task' ? 'indigo' : tag.type === 'goal' ? 'teal' : 'rose';
            const c = COLOR_MAP[tc];
            return (
              <span key={idx} className={`${c.bg} border ${c.border} ${c.text} text-xs px-3 py-2 rounded-xl flex items-center gap-2`}>
                <span className="font-bold">{tag.taskTitle}</span>
                <button onClick={() => removeTag(idx)} className="hover:text-rose-400 ml-1 text-lg leading-none">&times;</button>
              </span>
            );
          })}
          {tags.length === 0 && (
            <button
              onClick={() => setShowPicker(true)}
              className="w-full py-4 border-2 border-dashed border-slate-800 rounded-2xl text-slate-500 hover:text-indigo-400 hover:border-indigo-500/40 hover:bg-indigo-500/5 transition-all text-sm font-bold flex flex-col items-center justify-center gap-2"
            >
              <span className="text-2xl">🏅</span>
              Başarı Seçmek İçin Tıkla
            </button>
          )}
          {tags.length > 0 && (
            <button
              onClick={() => setShowPicker(true)}
              className="px-3 py-2 border border-slate-700 hover:border-indigo-500/40 rounded-xl text-slate-500 hover:text-indigo-400 text-xs font-bold transition-all"
            >
              + Ekle
            </button>
          )}
        </div>

        {}
        {showPicker && (
          <div className="bg-slate-800/80 border border-slate-700 rounded-2xl overflow-hidden mb-4 animate-in slide-in-from-top-2 duration-300">
            {}
            <div className="flex border-b border-slate-700">
              {ITEM_TYPES.map(t => {
                const c = COLOR_MAP[t.color];
                const isAct = pickerType === t.id;
                return (
                  <button
                    key={t.id}
                    onClick={() => setPickerType(t.id)}
                    className={`flex-1 py-3 text-xs font-black uppercase tracking-wider transition-all flex items-center justify-center gap-1.5 ${
                      isAct ? `${c.text} border-b-2 border-current bg-slate-800` : 'text-slate-500 hover:text-slate-300'
                    }`}
                  >
                    <span>{t.icon}</span>
                    <span className="hidden sm:inline">{t.label}</span>
                  </button>
                );
              })}
              <button onClick={() => setShowPicker(false)} className="px-4 text-slate-500 hover:text-white text-xs font-bold border-l border-slate-700">✕</button>
            </div>

            {}
            <div className="max-h-64 overflow-y-auto p-3 space-y-3 custom-scrollbar">
              {grouped.length === 0 ? (
                <p className="text-slate-500 text-xs italic py-6 text-center">
                  {pickerType === 'task'      && 'Son 7 günde tamamlanan görev yok. 💪'}
                  {pickerType === 'goal'      && 'Son 7 günde tamamlanan hedef yok. 🎯'}
                  {pickerType === 'challenge' && 'Son 7 günde biten mücadele yok. ⚔️'}
                </p>
              ) : grouped.map(([dayLbl, items]) => (
                <div key={dayLbl}>
                  <p className="text-[10px] text-slate-500 font-black uppercase tracking-widest mb-1.5 px-1">{dayLbl}</p>
                  <div className="space-y-1">
                    {items.map(item => {
                      const id = item.appTaskId || item.AppTaskId || item.id || item.Id;
                      const title = pickerType === 'task' ? (item.title || item.Title) : item.title;
                      const alreadyAdded = tags.some(t => t.taskId === id);
                      return (
                        <button
                          key={id}
                          onClick={() => handleSelectItem(item, pickerType)}
                          disabled={alreadyAdded}
                          className={`w-full text-left px-4 py-2.5 rounded-xl text-slate-200 text-xs transition-all flex items-center justify-between group ${
                            alreadyAdded
                              ? 'bg-slate-900/30 text-slate-600 cursor-not-allowed border border-slate-800'
                              : `bg-slate-900/50 hover:${cm.active} border border-slate-700 hover:${cm.border}`
                          }`}
                        >
                          <span className="font-bold truncate pr-2">{title}</span>
                          {alreadyAdded
                            ? <span className="text-emerald-500 text-[10px] font-black shrink-0">✓ Eklendi</span>
                            : <span className={`opacity-0 group-hover:opacity-100 transition-opacity ${cm.text} font-black shrink-0`}>SEÇ +</span>
                          }
                        </button>
                      );
                    })}
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}
      </div>

      {}
      <label className="text-[10px] text-slate-500 font-black uppercase tracking-widest mb-2 block">Deneyimini Anlat</label>
      <textarea
        value={content}
        onChange={(e) => setContent(e.target.value)}
        placeholder="Neler hissettin? Bu başarı seni nasıl geliştirdi? ✨"
        maxLength={500}
        rows={3}
        className="w-full bg-slate-800/50 border border-slate-700 rounded-2xl px-4 py-3 text-slate-200 placeholder-slate-600 focus:outline-none focus:border-indigo-500/50 resize-none text-sm mb-4"
      />

      {}
      <div className="mb-6">
        {!imagePreview ? (
          <label className="flex items-center gap-3 cursor-pointer text-slate-500 hover:text-indigo-400 transition-all text-xs font-bold bg-slate-800/30 hover:bg-indigo-500/5 border border-slate-800 hover:border-indigo-500/20 px-4 py-3 rounded-xl w-fit">
            <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
            </svg>
            Görsel Ekle (Opsiyonel)
            <input type="file" accept="image/*" onChange={handleImageChange} className="hidden" />
          </label>
        ) : (
          <div className="relative rounded-2xl overflow-hidden border border-indigo-500/30 group">
            <img src={imagePreview} alt="Önizleme" className="w-full h-auto max-h-[300px] object-cover" />
            <button
              onClick={() => { setImageFile(null); setImagePreview(null); }}
              className="absolute top-2 right-2 bg-rose-600 text-white p-2 rounded-xl shadow-lg opacity-0 group-hover:opacity-100 transition-all hover:bg-rose-500"
            >
              <svg className="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          </div>
        )}
      </div>

      <button
        onClick={handleSubmit}
        disabled={submitting || tags.length === 0}
        className="w-full bg-gradient-to-r from-indigo-600 to-violet-600 hover:from-indigo-500 hover:to-violet-500 disabled:opacity-30 disabled:grayscale disabled:cursor-not-allowed text-white font-black py-4 rounded-2xl transition-all duration-300 shadow-lg uppercase tracking-wider text-sm flex items-center justify-center gap-2"
      >
        {submitting ? 'Paylaşılıyor...' : <>Paylaş 🚀</>}
      </button>
    </div>
  );
};


const TabNavigator = ({ activeTab, setActiveTab }) => {
  const tabs = [
    { id: 'create', label: 'Paylaş',     icon: '✍️', color: 'indigo'  },
    { id: 'feed',   label: 'Akış',       icon: '👥', color: 'violet'  },
    { id: 'mine',   label: 'Başarılarım',icon: '🏆', color: 'emerald' },
  ];
  const activeIdx = tabs.findIndex(t => t.id === activeTab);

  return (
    
    <div className="max-w-xl mx-auto relative flex p-1.5 bg-slate-950/90 backdrop-blur-2xl border border-slate-800/50 rounded-[2rem] mb-10 shadow-2xl shadow-black/40">
      {}
      <div
        className="absolute h-[calc(100%-12px)] top-1.5 rounded-2xl transition-all duration-500 shadow-lg z-0"
        style={{
          width: `calc(${100 / tabs.length}% - 8px)`,
          left: `calc(${activeIdx * (100 / tabs.length)}% + 4px)`,
          background: activeTab === 'create' ? 'rgba(79,70,229,0.25)' : activeTab === 'feed' ? 'rgba(139,92,246,0.25)' : 'rgba(16,185,129,0.25)',
          border: `1px solid ${activeTab === 'create' ? 'rgba(79,70,229,0.4)' : activeTab === 'feed' ? 'rgba(139,92,246,0.4)' : 'rgba(16,185,129,0.4)'}`,
        }}
      />
      {tabs.map(tab => (
        <button
          key={tab.id}
          onClick={() => setActiveTab(tab.id)}
          className={`relative z-10 flex-1 flex flex-col sm:flex-row items-center justify-center gap-2 py-3 rounded-2xl text-[10px] sm:text-xs font-black uppercase tracking-[0.1em] transition-all duration-300 ${
            activeTab === tab.id ? 'text-white' : 'text-slate-500 hover:text-slate-300'
          }`}
        >
          <span className={`text-lg transition-transform duration-300 ${activeTab === tab.id ? 'scale-125' : ''}`}>
            {tab.icon}
          </span>
          <span className="opacity-80">{tab.label}</span>
          {activeTab === tab.id && (
            <span className="absolute -bottom-1 w-1 h-1 rounded-full bg-current animate-pulse" />
          )}
        </button>
      ))}
    </div>
  );
};


const SocialFeedPage = () => {
  const { user } = useAuth();
  const currentUserId = user?.userId || user?.id || null;
  const [posts, setPosts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [userTasks, setUserTasks] = useState([]);
  const [completedGoals, setCompletedGoals] = useState([]);
  const [completedChallenges, setCompletedChallenges] = useState([]);
  const [activeTab, setActiveTab] = useState('feed'); 

  const fetchFeed = useCallback(async () => {
    try {
      const data = await postService.getFeed();
      setPosts(data.posts || []);
    } catch (err) {
      console.error('Feed yüklenemedi:', err);
    } finally {
      setLoading(false);
    }
  }, []);

  
  
  
  const fetchCompletedTasks = useCallback(async () => {
    try {
      const allCompleted = [];
      
      const pagePromises = Array.from({ length: 7 }, (_, i) =>
        taskService.getAllTasks(null, i + 1, 100).catch(() => null)
      );
      const results = await Promise.all(pagePromises);

      for (const res of results) {
        if (!res) continue;
        const tasks = Array.isArray(res) ? res : (res?.tasks || res?.Tasks || []);
        const completed = tasks.filter(t =>
          t.status === 'Completed' || t.Status === 'Completed'
          || t.isCompleted === true || t.IsCompleted === true
        );
        allCompleted.push(...completed);
      }

      
      const seenIds = new Set();
      const seenAppTaskIds = new Set();
      const deduplicated = allCompleted.filter(t => {
        const uid = t.id || t.Id;
        const atid = t.appTaskId || t.AppTaskId;

        
        if (uid && seenIds.has(uid)) return false;
        if (uid) seenIds.add(uid);

        
        if (atid && seenAppTaskIds.has(atid)) return false;
        if (atid) seenAppTaskIds.add(atid);

        return true;
      });

      setUserTasks(deduplicated);
    } catch (err) {
      console.error('Tamamlanan görevler yüklenemedi:', err);
      setUserTasks([]);
    }
  }, []);

  useEffect(() => {
    fetchCompletedTasks();

    import('../services/goalService').then(({ default: goalService }) => {
      goalService.getGoalDashboard().then((data) => {
        setCompletedGoals(data?.completedGoals || []);
      }).catch(() => {});
    });

    import('../services/challengeService').then(({ challengeService }) => {
      challengeService.getActiveChallenges('completed').then((data) => {
        setCompletedChallenges(data?.challenges || data || []);
      }).catch(() => {});
    });

    fetchFeed();
  }, [fetchFeed, fetchCompletedTasks]);

  const handleDelete = async (postId) => {
    if (!window.confirm('Bu gönderiyi silmek istediğine emin misin?')) return;
    try {
      await postService.deletePost(postId);
      toast.success('Gönderi silindi.');
      setPosts((prev) => prev.filter((p) => p.id !== postId));
    } catch (err) {
      toast.error('Silme işlemi başarısız.');
    }
  };

  
  const myPosts = posts.filter(p => p.author.id === currentUserId);
  const friendsPosts = posts.filter(p => p.author.id !== currentUserId);

  return (
    <MainLayout>
      <div className="container mx-auto px-4 py-8">
        {}
        <section className="mb-10 text-center lg:text-left">
          <h2 className="text-4xl lg:text-5xl font-black text-white italic tracking-tighter mb-2">SOSYAL AKIŞ</h2>
          <div className="h-1.5 w-24 bg-indigo-600 rounded-full shadow-[0_0_20px_rgba(79,70,229,0.6)] mx-auto lg:mx-0" />
          <p className="text-slate-500 text-sm mt-4">Toplulukla etkileşime geç, başarılarını kutla.</p>
        </section>

        {}
        <TabNavigator activeTab={activeTab} setActiveTab={setActiveTab} />

        {}
        <div className="max-w-4xl mx-auto">

          {}
          {activeTab === 'create' && (
            <div className="max-w-2xl mx-auto">
              <h3 className="text-[10px] text-slate-500 font-black uppercase tracking-[0.2em] mb-4 flex items-center gap-2">
                <span className="w-1.5 h-1.5 bg-indigo-500 rounded-full" />
                Yeni Paylaşım Oluştur
              </h3>
              <CreatePostForm
                onPostCreated={() => { fetchFeed(); setActiveTab('feed'); }}
                userTasks={userTasks}
                completedGoals={completedGoals}
                completedChallenges={completedChallenges}
              />
            </div>
          )}

          {}
          {activeTab === 'feed' && (
            <div className="max-w-2xl mx-auto">
              <h3 className="text-[10px] text-slate-500 font-black uppercase tracking-[0.2em] mb-4 flex items-center gap-2">
                <span className="w-1.5 h-1.5 bg-violet-500 rounded-full" />
                Global Akış
              </h3>
              {loading ? (
                <div className="space-y-6">
                  {[1, 2].map(i => <div key={i} className="h-64 bg-slate-900/40 rounded-3xl animate-pulse border border-slate-800" />)}
                </div>
              ) : friendsPosts.length === 0 ? (
                <div className="bg-slate-900/40 border border-slate-800 rounded-3xl p-12 text-center">
                  <p className="text-4xl mb-4 opacity-50">👥</p>
                  <p className="text-slate-400 font-bold italic">Henüz bir hareket yok!</p>
                  <p className="text-slate-600 text-xs mt-2">Arkadaş edinerek akışını canlandırabilirsin.</p>
                </div>
              ) : (
                <div className="space-y-8">
                  {friendsPosts.map(post => (
                    <PostCard key={post.id} post={post} currentUserId={currentUserId} onDelete={handleDelete} />
                  ))}
                </div>
              )}
            </div>
          )}

          {}
          {activeTab === 'mine' && (
            <div className="max-w-2xl mx-auto">
              <h3 className="text-[10px] text-slate-500 font-black uppercase tracking-[0.2em] mb-4 flex items-center gap-2">
                <span className="w-1.5 h-1.5 bg-emerald-500 rounded-full" />
                Başarılarım ve İlerlemem
              </h3>
              {loading ? (
                <div className="h-48 bg-slate-900/40 rounded-3xl animate-pulse border border-slate-800" />
              ) : myPosts.length === 0 ? (
                <div className="bg-slate-900/40 border border-slate-800 rounded-3xl p-8 text-center">
                  <p className="text-3xl mb-3 opacity-50">⭐</p>
                  <p className="text-slate-400 text-sm font-bold">Henüz bir paylaşımın yok.</p>
                  <p className="text-slate-600 text-xs mt-2">İlk başarını paylaşarak seriyi başlat!</p>
                </div>
              ) : (
                <div className="space-y-6">
                  {myPosts.map(post => (
                    <PostCard key={post.id} post={post} currentUserId={currentUserId} onDelete={handleDelete} />
                  ))}
                </div>
              )}
            </div>
          )}

        </div>
      </div>
      <ToastContainer position="bottom-right" theme="dark" toastClassName="!rounded-2xl" />
    </MainLayout>
  );
};

export default SocialFeedPage;
